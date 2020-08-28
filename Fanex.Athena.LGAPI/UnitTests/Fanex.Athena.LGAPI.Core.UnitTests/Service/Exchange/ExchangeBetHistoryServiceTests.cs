using System;
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
    public class ExchangeBetHistoryServiceTests
    {
        private static Fixture _fixture = new Fixture();

        private ICitiXchangeClient _apiClient;
        private ISyncPolicy<BetHistory> _breaker;
        private ICacheService _cacheService;

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const int CodeForUM = 101;
        private const string Environment = "DEV";
        private const string BetIdOfOneTwoBet = "1";
        private const string BetIdOfALog = "2";

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

        [TestCase(SiteName.OneTwoBet, true, "", CodeForUM)]
        [TestCase("WrongSitename", false, "", (int)CitiStatus.Success)]
        [TestCase(SiteName.OneTwoBet, false, "abc", (int)CitiStatus.Success)]
        [TestCase(SiteName.OneTwoBet, false, "5000000000", (int)CitiStatus.Success)]
        public void GetVoidedData_InvalidParamOrInUMTime_ReturnCorrectResult(string siteName, bool isUM, string refNo, int expectedErrCode)
        {
            // Arrange
            var metaModel = CreateMetaModel(siteName: siteName, refNo: refNo);

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
                .queryBetHistory(metaModel.CustId, string.Empty, string.Empty, BetStatusHistory.VOIDED, false, metaModel.PageIndex, metaModel.ReportType, Enviro.POC);
            });
        }

        [Test]
        public void GetVoidedData_CheckUMButThrowException_UMIsTrueAndReturnCorrectResult()
        {
            // Arrange
            var metaModel = CreateMetaModel();

            _apiClient.isUnderMaintainence().Returns(x => throw new Exception());

            var service = new ExchangeBetHistoryService(_apiClient, _breaker, _cacheService);

            // Action
            var result = service.GetVoidedData(metaModel);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.ErrCode, Is.EqualTo(CodeForUM));
                Assert.That(result.DataList.Count, Is.Zero);
                _apiClient.DidNotReceiveWithAnyArgs()
                .queryBetHistory(metaModel.CustId, string.Empty, string.Empty, BetStatusHistory.VOIDED, false, metaModel.PageIndex, metaModel.ReportType, Enviro.POC);
            });
        }

        private static ExchangeGettingVoidedMetaModel CreateMetaModel(string siteName = SiteName.OneTwoBet, string refNo = "", int custId = 0)
        {
            var metaModel = _fixture.Create<ExchangeGettingVoidedMetaModel>();
            metaModel.SiteName = siteName;
            metaModel.Environment = Environment;
            metaModel.RefNo = refNo;
            metaModel.CustId = custId;

            return metaModel;
        }
    }
}
