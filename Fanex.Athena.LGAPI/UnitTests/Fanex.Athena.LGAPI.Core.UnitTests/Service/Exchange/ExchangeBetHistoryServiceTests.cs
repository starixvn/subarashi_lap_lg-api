using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Athena.LGAPI.Core.Model.DTO.Exchange;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;
using Fanex.Athena.LGAPI.Core.Service.Exchange;
using Fanex.Caching;
using Fanex.Logging;
using Fanex.ThirdParty.ExchangeProduct.Enums;
using Fanex.ThirdParty.ExchangeProduct.HttpClients;
using Fanex.ThirdParty.ExchangeProduct.Model;
using NSubstitute;
using NUnit.Framework;
using Polly;

namespace Fanex.Athena.LGAPI.Core.UnitTests.Service.Exchange
{
    [TestFixture]
    public class ExchangeBetHistoryServiceTests
    {
        private static Fixture _fixture = new Fixture();

        private IExchangeHttpClient _apiClient;
        private IAsyncPolicy<BetHistory> _breaker;
        private ICacheService _cacheService;
        private ILogger _logger;

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const int CodeForUM = 101;
        private const string Environment = "DEV";
        private const string BetIdOfOneTwoBet = "1";
        private const string BetIdOfALog = "2";

        [SetUp]
        public void SetUp()
        {
            _apiClient = Substitute.For<IExchangeHttpClient>();
            _breaker = Policy.NoOpAsync<BetHistory>();
            _cacheService = Substitute.For<ICacheService>();
            _logger = Substitute.For<ILogger>();
        }

        [TearDown]
        public void TearDown()
        {
            _apiClient = null;
            _breaker = null;
            _cacheService = null;
        }

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

            var service = new ExchangeBetHistoryService(_apiClient, _breaker, _cacheService, _logger);
            service.ExchangeLicenseeMappings().Returns(listSiteName);

            // Action
            var result = await service.GetVoidedData(metaModel);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.ErrCode, Is.EqualTo(expectedErrCode));
                Assert.That(result.DataList.Count, Is.Zero);
                _apiClient.DidNotReceiveWithAnyArgs()
                .QueryBetHistoryForAllStatusAsync(default);
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