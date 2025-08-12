namespace Fanex.Athena.LGAPI.Core.UnitTests.Service.Exchange
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Caching;
    using Core.Service.Exchange;
    using Data;
    using Infrastructure;
    using Logging;
    using Model.DTO.Exchange;
    using Model.MetalModel.Exchange;
    using NSubstitute;
    using NUnit.Framework;
    using Polly;
    using ThirdParty.ExchangeProduct.Enums;
    using ThirdParty.ExchangeProduct.HttpClients;
    using ThirdParty.ExchangeProduct.Model;
    using ThirdParty.ExchangeProduct.Params;

    [TestFixture]
    public class ExchangeBetHistoryServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            _apiClient = Substitute.For<IExchangeHttpClient>();
            _breaker = Policy.NoOpAsync<BetHistory>();
            _cacheService = Substitute.For<ICacheService>();
            _logger = Substitute.For<ILogger>();
            _dbFactory = Substitute.For<IDbFactory>();
            _objectDb = Substitute.For<IObjectDb>();

            _dbFactory.GetObjectDb(Arg.Any<string>(), Arg.Any<int>()).Returns(_objectDb);
            _objectDb.ClearReceivedCalls();
        }

        [TearDown]
        public void TearDown()
        {
            _apiClient = null;
            _breaker = null;
            _cacheService = null;
            _objectDb.ClearReceivedCalls();
        }

        private static readonly Fixture _fixture = new Fixture();

        private IExchangeHttpClient _apiClient;
        private IAsyncPolicy<BetHistory> _breaker;
        private ICacheService _cacheService;
        private ILogger _logger;
        private IDbFactory _dbFactory;
        private IObjectDb _objectDb;

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const int CodeForUM = 101;
        private const string Environment = "DEV";
        private const string BetIdOfOneTwoBet = "1";
        private const string BetIdOfALog = "2";

        [TestCase("WrongSitename", false, "", (int)CitiStatus.Success)]
        public async Task GetVoidedData_InvalidParam_ReturnCorrectResult(string siteName, bool isValidSiteName, string refNo, int expectedErrCode)
        {
            // Arrange
            var metaModel = CreateMetaModel(siteName: siteName, refNo: refNo);
            var listSiteName = new List<ExchangeLicenseeMapping>();
            if (isValidSiteName)
            {
                listSiteName.Add(new ExchangeLicenseeMapping { SiteId = 1, SiteName = siteName });
            }

            var service = new ExchangeBetHistoryService(_apiClient, _breaker, _cacheService, _logger, _dbFactory);
            _objectDb.Query<ExchangeLicenseeMapping>(Arg.Any<object>()).Returns(listSiteName);

            // Action
            var result = await service.GetVoidedData(metaModel);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.ErrCode, Is.EqualTo(expectedErrCode));
                Assert.That(result.DataList.Count, Is.Zero);
                _apiClient.DidNotReceiveWithAnyArgs()
                    .QueryBetHistoryForAllStatusAsync(Arg.Any<QueryBetHistoryForAllStatusParam>());
            });
        }

        private static ExchangeGettingVoidedMetaModel CreateMetaModel(string siteName = "12bet", string refNo = "", int custId = 0)
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