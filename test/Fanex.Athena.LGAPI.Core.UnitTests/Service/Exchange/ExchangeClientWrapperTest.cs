namespace Fanex.Athena.LGAPI.Core.UnitTests.Service.Exchange
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using Caching;
    using Core.Service.Exchange;
    using Infrastructure;
    using Logging;
    using NSubstitute;
    using NSubstitute.ExceptionExtensions;
    using NUnit.Framework;
    using ThirdParty.ExchangeProduct;
    using ThirdParty.ExchangeProduct.HttpClients;
    using ThirdParty.ExchangeProduct.Model;
    using ThirdParty.ExchangeProduct.Params;

    [TestFixture]
    public class ExchangeClientWrapperTest
    {
        [SetUp]
        public void SetUp()
        {
            _apiClient = Substitute.For<IExchangeHttpClient>();
            _logger = Substitute.For<ILogger>();
            _cacheService = Substitute.For<ICacheService>();
            _dbFactory = Substitute.For<IDbFactory>();
        }

        [TearDown]
        public void TearDown()
        {
            _apiClient = null;
            _logger = null;
            _cacheService = null;
        }

        private readonly Fixture _fixture = new Fixture();

        private IExchangeHttpClient _apiClient;

        private ILogger _logger;

        private ICacheService _cacheService;
        private IDbFactory _dbFactory;

        private const int CustId = 1;

        [Test]
        public async Task GetCommission_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var expectedResult = _fixture.Create<GetProfile>();

            _apiClient.GetProfileAsync(Arg.Any<GetProfileParam>()).ReturnsForAnyArgs(new ResponseResult<GetProfile> { Result = expectedResult });

            var wrapper = CreateExchangeClientWrapper();

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
            _apiClient.GetProfileAsync(Arg.Any<GetProfileParam>()).ThrowsForAnyArgs(x => throw new Exception(ErrMsg));

            var wrapper = CreateExchangeClientWrapper();

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
            _apiClient.UpdateCommissionAsync(Arg.Any<UpdateCommissionParam>())
                .ReturnsForAnyArgs(Task.FromResult(new ResponseResult<CommissionInfo> { Result = expectedResult }));

            var wrapper = CreateExchangeClientWrapper();

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
            _apiClient.UpdateCommissionAsync(Arg.Any<UpdateCommissionParam>())
                .ThrowsForAnyArgs(x => throw new Exception(ErrMsg));

            var wrapper = CreateExchangeClientWrapper();

            // Action
            var result = await wrapper.UpdateCommission(CustId, NewCommisison, 0);

            // Assert
            Assert.That(result, Is.Null);
            _logger.Received(1).Error(Arg.Any<string>(), Arg.Any<Exception>());
        }

        private ExchangeClientWrapper CreateExchangeClientWrapper()
        {
            return new ExchangeClientWrapper(_apiClient, _logger, _cacheService, _dbFactory);
        }
    }
}