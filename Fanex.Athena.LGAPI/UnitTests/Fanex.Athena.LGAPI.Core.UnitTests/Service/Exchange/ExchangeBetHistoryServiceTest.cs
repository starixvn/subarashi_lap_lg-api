using System.Collections.Generic;
using AutoFixture;
using CitiXchangeLibrary;
using Fanex.Athena.LGAPI.Core.Model.DataDomain;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;
using Fanex.Athena.LGAPI.Core.Service.Exchange;
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

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const int CodeForUM = 101;
        private const string Environment = "DEV";
        private const int BetIdOfOneTwoBet = 1;
        private const int BetIdOfALog = 2;
        private const int CustIdOfOneTwoBet = 1063580;

        [SetUp]
        public void SetUp()
        {
            _apiClient = Substitute.For<ICitiXchangeClient>();
            _breaker = Policy.NoOp<BetHistory>();
        }

        [TearDown]
        public void TearDown()
        {
            _apiClient = null;
            _breaker = null;
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

            var startDate = metaModel.SelectedDate.Date.ToString(DateTimeFormat);
            var endDate = metaModel.SelectedDate.Date.AddDays(1).AddSeconds(-1).ToString(DateTimeFormat);

            _apiClient.isUnderMaintainence().Returns(false);

            var expectedApiData = CreateFakeData();
            _apiClient.queryBetHistory(metaModel.CustId, startDate, endDate, BetStatusHistory.VOIDED, false, metaModel.PageIndex, Enviro.POC)
                .Returns(expectedApiData);

            var service = new ExchangeBetHistoryService(_apiClient, _breaker);

            // Action
            var result = service.GetVoidedData(metaModel);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.TotalPage, Is.EqualTo(1));
                Assert.That(result.DataList[0].RefNo, Is.EqualTo(expectedRefNo));
            });
        }

        [TestCase(SiteName.OneTwoBet, true, CodeForUM)]
        [TestCase("WrongSitename", false, 0)]
        public void GetVoidedData_WrongSiteNameOrInUMTime_ReturnCorrectResult(string siteName, bool isUM, int expectedErrCode)
        {
            // Arrange
            var metaModel = _fixture.Create<ExchangeGettingVoidedMetaModel>();
            metaModel.SiteName = siteName;
            metaModel.Environment = Environment;
            metaModel.CustId = 0;
            metaModel.RefNo = 0;

            _apiClient.isUnderMaintainence().Returns(isUM);

            var service = new ExchangeBetHistoryService(_apiClient, _breaker);

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

            var service = new ExchangeBetHistoryService(_apiClient, _breaker);

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

        [TestCase(CustIdOfOneTwoBet, 0, true)]
        [TestCase(0, BetIdOfOneTwoBet, true)]
        [TestCase(123, BetIdOfOneTwoBet, false)]
        [TestCase(123, 0, false)]
        [TestCase(0, 123, false)]
        public void GetVoidedData_HasFilter_ReturnCorrectResult(int custId, int refNo, bool hasData)
        {
            // Arrange
            var metaModel = _fixture.Create<ExchangeGettingVoidedMetaModel>();
            metaModel.SiteName = SiteName.OneTwoBet;
            metaModel.Environment = Environment;
            metaModel.CustId = custId;
            metaModel.RefNo = refNo;

            var startDate = metaModel.SelectedDate.Date.ToString(DateTimeFormat);
            var endDate = metaModel.SelectedDate.Date.AddDays(1).AddSeconds(-1).ToString(DateTimeFormat);

            _apiClient.isUnderMaintainence().Returns(false);

            var expectedApiData = CreateFakeData();
            _apiClient.queryBetHistory(metaModel.CustId, startDate, endDate, BetStatusHistory.VOIDED, false, metaModel.PageIndex, Enviro.POC)
                .Returns(expectedApiData);

            var service = new ExchangeBetHistoryService(_apiClient, _breaker);

            // Action
            var resultData = service.GetVoidedData(metaModel);
            var result = resultData.DataList.Count > 0;

            // Assert
            Assert.That(result, Is.EqualTo(hasData));
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
                        userId = CustIdOfOneTwoBet.ToString(),
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