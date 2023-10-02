using System;
using System.Threading.Tasks;
using Fanex.Athena.LGAPI.Core.Service.Exchange;
using Fanex.Caching;
using Fanex.Logging;
using Fanex.ThirdParty.ExchangeProduct;
using Fanex.ThirdParty.ExchangeProduct.HttpClients;
using Fanex.ThirdParty.ExchangeProduct.Model;
using Fanex.ThirdParty.ExchangeProduct.Params;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Fanex.Athena.LGAPI.Core.UnitTests.Service.Exchange
{
    [TestFixture]
    public class ExchangeClientWrapperTest
    {
        private IExchangeHttpClient _apiClient;

        private ILogger _logger;

        private ICacheService _cacheService;
        private const int CustId = 1;

        [SetUp]
        public void SetUp()
        {
            _apiClient = Substitute.For<IExchangeHttpClient>();
            _logger = Substitute.For<ILogger>();
            _cacheService = Substitute.For<ICacheService>();
        }

        [TearDown]
        public void TearDown()
        {
            _apiClient = null;
            _logger = null;
            _cacheService = null;
        }

        [Test]
        public async Task GetCommission_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var expectedResult = new ResponseResult<GetProfile> { Result = new GetProfile() };
            _apiClient.GetProfileAsync(default).ReturnsForAnyArgs(expectedResult);

            var wrapper = new ExchangeClientWrapper(_apiClient, _logger, _cacheService);

            // Action
            var result = await wrapper.GetCommission(CustId, 0);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public async Task GetCommission_ThrowException_LogCorrectInfo()
        {
            // Arrange
            const string ErrMsg = "GetCommission - ThrowException";
            _apiClient.GetProfileAsync(default).ThrowsForAnyArgs(x => throw new Exception(ErrMsg));

            var wrapper = new ExchangeClientWrapper(_apiClient, _logger, _cacheService);

            // Action
            var result = await wrapper.GetCommission(CustId, 0);

            // Assert
            Assert.That(result, Is.Null);
            _logger.Received(1).Error(Arg.Any<string>(), Arg.Any<Exception>());
        }

        [Test]
        public async Task UpdateCommission_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            const double NewCommisison = 0.5;
            var expectedResult = new CommissionInfo();
             _apiClient.UpdateCommissionAsync(default).ReturnsForAnyArgs(Task.FromResult(new ResponseResult<CommissionInfo> { Result = expectedResult }));

            var wrapper = new ExchangeClientWrapper(_apiClient, _logger, _cacheService);

            // Action
            var result = await wrapper.UpdateCommission(CustId, NewCommisison, 0);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public async Task UpdateCommission_ThrowException_LogCorrectInfo()
        {
            // Arrange
            const string ErrMsg = "UpdateCommission - ThrowException";
            const double NewCommisison = 0.5;
            _apiClient.UpdateCommissionAsync(default)
                .ThrowsForAnyArgs(x => throw new Exception(ErrMsg));

            var wrapper = new ExchangeClientWrapper(_apiClient, _logger, _cacheService);

            // Action
            var result = await wrapper.UpdateCommission(CustId, NewCommisison, 0);

            // Assert
            Assert.That(result, Is.Null);
            _logger.Received(1).Error(Arg.Any<string>(), Arg.Any<Exception>());
        }
    }
}