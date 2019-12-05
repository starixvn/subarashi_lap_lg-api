using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using CitiXchangeLibrary;
using Fanex.Athena.LGAPI.Core.Model.DataDomain;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;
using Fanex.Athena.LGAPI.Core.Service.Exchange;
using Fanex.Caching;
using NSubstitute;
using NUnit.Framework;
using Polly;

namespace Fanex.Athena.LGAPI.Core.UnitTests.Service.Exchange
{
    [TestFixture]
    public class ExchangeBetHistoryServiceTest
    {
        private static Fixture _fixture = new Fixture();

        private ICitiXchangeClient _apiClient;

        private ISyncPolicy<BetHistory> _breaker;

        private ICacheService _cacheService;

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const int CodeForUM = 101;
        private const string Environment = "DEV";
        private const int BetIdOfOneTwoBet = 1;
        private const int BetIdOfALog = 2;

        [SetUp]
        public void SetUp()
        {
            _apiClient = Substitute.For<ICitiXchangeClient>();
            _breaker = Policy.NoOp<BetHistory>();
            _cacheService = Substitute.For<ICacheService>();
        }

        [TearDown]
        public void TearDown()
        {
            _apiClient = null;
            _breaker = null;
            _cacheService = null;
        }

        [TestCase(SiteName.OneTwoBet, BetIdOfOneTwoBet)]
        [TestCase(SiteName.ALog, BetIdOfALog)]
        public void GetVoidedData_NotInUMTime_ReturnCorrectResult(string siteName, int expectedRefNo)
        {
            // Arrange
            var metaModel = _fixture.Create<ExchangeGettingVoidedMetaModel>();
            metaModel.SiteName = siteName;
            metaModel.Environment = Environment;
            metaModel.CustId = 0;
            metaModel.RefNo = 0;

            _apiClient.isUnderMaintainence().Returns(false);
            FakeApiClient(metaModel);

            var service = new ExchangeBetHistoryService(_apiClient, _breaker, _cacheService);

            // Action
            var result = service.GetVoidedData(metaModel);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.TotalPage, Is.EqualTo(1));
                Assert.That(result.DataList.ElementAt(0).RefNo, Is.EqualTo(expectedRefNo));
            });
        }

        [TestCase(SiteName.OneTwoBet, true, CodeForUM)]
        [TestCase("WrongSitename", false, (int)CitiStatus.Success)]
        public void GetVoidedData_WrongSiteNameOrInUMTime_ReturnCorrectResult(string siteName, bool isUM, int expectedErrCode)
        {
            // Arrange
            var metaModel = _fixture.Create<ExchangeGettingVoidedMetaModel>();
            metaModel.SiteName = siteName;
            metaModel.Environment = Environment;
            metaModel.CustId = 0;
            metaModel.RefNo = 0;

            _apiClient.isUnderMaintainence().Returns(isUM);

            var service = new ExchangeBetHistoryService(_apiClient, _breaker, _cacheService);

            // Action
            var result = service.GetVoidedData(metaModel);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.ErrCode, Is.EqualTo(expectedErrCode));
                Assert.That(result.DataList.Count, Is.Zero);
                _apiClient.DidNotReceiveWithAnyArgs()
                .queryBetHistory(metaModel.CustId, string.Empty, string.Empty, BetStatusHistory.VOIDED, false, metaModel.PageIndex, Enviro.POC);
            });
        }

        [Test]
        public void GetVoidedData_CheckUMButThrowException_UMIsTrueAndReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<ExchangeGettingVoidedMetaModel>();
            metaModel.SiteName = SiteName.OneTwoBet;
            metaModel.Environment = Environment;
            metaModel.CustId = 0;
            metaModel.RefNo = 0;

            _apiClient.isUnderMaintainence().Returns(x => throw new System.Exception());

            var service = new ExchangeBetHistoryService(_apiClient, _breaker, _cacheService);

            // Action
            var result = service.GetVoidedData(metaModel);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.ErrCode, Is.EqualTo(CodeForUM));
                Assert.That(result.DataList.Count, Is.Zero);
                _apiClient.DidNotReceiveWithAnyArgs()
                .queryBetHistory(metaModel.CustId, string.Empty, string.Empty, BetStatusHistory.VOIDED, false, metaModel.PageIndex, Enviro.POC);
            });
        }

        [TestCase(0, true)]
        [TestCase(BetIdOfOneTwoBet, true)]
        [TestCase(123, false)]
        public void GetVoidedData_HasFilter_ReturnCorrectResult(int refNo, bool hasData)
        {
            // Arrange
            var metaModel = _fixture.Create<ExchangeGettingVoidedMetaModel>();
            metaModel.SiteName = SiteName.OneTwoBet;
            metaModel.Environment = Environment;
            metaModel.RefNo = refNo;

            _apiClient.isUnderMaintainence().Returns(false);
            FakeApiClient(metaModel);

            var service = new ExchangeBetHistoryService(_apiClient, _breaker, _cacheService);

            // Action
            var resultData = service.GetVoidedData(metaModel);
            var result = resultData.DataList.Count() > 0;

            // Assert
            Assert.That(result, Is.EqualTo(hasData));
        }

        [TestCase(0, -1, CitiStatus.Success, true)]
        [TestCase(0, -10, CitiStatus.Success, true)]
        [TestCase(0, -1, CitiStatus.Not_Authorized, false)]
        [TestCase(0, 0, CitiStatus.Success, false)]
        [TestCase(0, 1, CitiStatus.Success, false)]
        [TestCase(123, -1, CitiStatus.Success, false)]
        public void GetVoidedData_CachingByConditionForApiData_CacheDataCorrectly(int custId, int offsetDay, CitiStatus status, bool shouldBeCached)
        {
            // Arrange
            var metaModel = _fixture.Create<ExchangeGettingVoidedMetaModel>();
            metaModel.SiteName = SiteName.OneTwoBet;
            metaModel.Environment = Environment;
            metaModel.CustId = custId;
            metaModel.SelectedDate = DateTime.Now.Date.AddDays(offsetDay);

            _apiClient.isUnderMaintainence().Returns(false);

            var expectedApiData = CreateFakeData(status);
            FakeApiClient(metaModel, expectedApiData);

            var cacheKey = $"ExchangeVoided_{metaModel.SelectedDate.ToString("yyyyMMdd")}_{metaModel.PageIndex}";
            var service = new ExchangeBetHistoryService(_apiClient, _breaker, _cacheService);

            // Action
            service.GetVoidedData(metaModel);

            // Assert
            if (shouldBeCached)
            {
                _cacheService.Received(1).Set(cacheKey, expectedApiData, Arg.Any<CacheItemOptions>());
            }
            else
            {
                _cacheService.DidNotReceiveWithAnyArgs().Set(Arg.Any<string>(), Arg.Any<BetHistory>(), Arg.Any<CacheItemOptions>());
            }
        }

        [Test]
        public void GetVoidedData_SearchAllAndApiDataInCache_CheckCacheFirstAndNotCallApiClient()
        {
            // Arrange
            var metaModel = _fixture.Create<ExchangeGettingVoidedMetaModel>();
            metaModel.SiteName = SiteName.OneTwoBet;
            metaModel.Environment = Environment;
            metaModel.CustId = 0;
            metaModel.RefNo = 0;
            metaModel.SelectedDate = DateTime.Now.Date.AddDays(-1);

            _apiClient.isUnderMaintainence().Returns(false);

            var expectedApiData = CreateFakeData();
            var cacheKey = $"ExchangeVoided_{metaModel.SelectedDate.ToString("yyyyMMdd")}_{metaModel.PageIndex}";
            _cacheService.Get<BetHistory>(cacheKey).Returns(expectedApiData);

            var service = new ExchangeBetHistoryService(_apiClient, _breaker, _cacheService);

            // Action
            var result = service.GetVoidedData(metaModel);

            // Assert
            _cacheService.Received(1).Get<BetHistory>(cacheKey);
            _apiClient.DidNotReceiveWithAnyArgs()
                .queryBetHistory(
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                BetStatusHistory.VOIDED,
                false,
                metaModel.PageIndex,
                Enviro.POC);
            Assert.That(result.DataList.Any(), Is.True);
        }

        [Test]
        public void GetVoidedData_SearchWithCustId_CallApiClientAndNotCheckApiDataFromCache()
        {
            // Arrange
            var metaModel = _fixture.Create<ExchangeGettingVoidedMetaModel>();
            metaModel.SiteName = SiteName.OneTwoBet;
            metaModel.Environment = Environment;
            metaModel.RefNo = 0;
            metaModel.CustId = 1;
            metaModel.SelectedDate = DateTime.Now.Date.AddDays(-1);

            _apiClient.isUnderMaintainence().Returns(false);

            var cacheKey = $"ExchangeVoided_{metaModel.SelectedDate.ToString("yyyyMMdd")}_{metaModel.PageIndex}";
            var service = new ExchangeBetHistoryService(_apiClient, _breaker, _cacheService);

            // Action
            service.GetVoidedData(metaModel);

            // Assert
            _cacheService.DidNotReceiveWithAnyArgs().Get<BetHistory>(cacheKey);
            _apiClient.Received(1)
                .queryBetHistory(
                1,
                Arg.Any<string>(),
                Arg.Any<string>(),
                BetStatusHistory.VOIDED,
                false,
                metaModel.PageIndex,
                Enviro.POC);
        }

        [Test]
        public void GetVoidedData_ApiDataReturnEmptyResult_ReturnEmptyDataList()
        {
            // Arrange
            var metaModel = _fixture.Create<ExchangeGettingVoidedMetaModel>();
            metaModel.SiteName = SiteName.OneTwoBet;
            metaModel.Environment = Environment;
            metaModel.RefNo = 0;
            metaModel.CustId = 0;
            metaModel.SelectedDate = DateTime.Now.Date.AddDays(-1);

            _apiClient.isUnderMaintainence().Returns(false);
            var apiData = CreateFakeData();
            apiData.resultList = new List<BetHistoryResultList>();
            FakeApiClient(metaModel, apiData);

            var service = new ExchangeBetHistoryService(_apiClient, _breaker, _cacheService);

            // Action
            var resultData = service.GetVoidedData(metaModel);

            // Assert
            Assert.That(resultData.DataList, Is.Empty);
        }

        private void FakeApiClient(ExchangeGettingVoidedMetaModel metaModel, BetHistory apiData = null)
        {
            var startDate = metaModel.SelectedDate.Date.ToString(DateTimeFormat);
            var endDate = metaModel.SelectedDate.Date.AddDays(1).AddSeconds(-1).ToString(DateTimeFormat);

            var expectedApiData = apiData ?? CreateFakeData();
            _apiClient.queryBetHistory(metaModel.CustId, startDate, endDate, BetStatusHistory.VOIDED, false, metaModel.PageIndex, Enviro.POC)
                .Returns(expectedApiData);
        }

        private static BetHistory CreateFakeData(CitiStatus status = CitiStatus.Success)
        {
            return new BetHistory
            {
                currentPage = 1,
                totalPage = 1,
                status = status,
                resultList = new List<BetHistoryResultList>
                {
                    new BetHistoryResultList
                    {
                        betId = BetIdOfOneTwoBet,
                        eventTypeName = "Event Type 1",
                        eventName = "Liverppol vs ManCity",
                        betStatus = "voided",
                        userId = "1063580",
                        extension1 = ExchangeSiteId.OneTwoBet
                    },
                    new BetHistoryResultList
                    {
                        betId = BetIdOfALog,
                        eventTypeName = "Event Type 1",
                        eventName = "MU vs Barca",
                        betStatus = "voided",
                        userId = "3227401",
                        extension1 = ExchangeSiteId.ALog
                    }
                }
            };
        }
    }
}