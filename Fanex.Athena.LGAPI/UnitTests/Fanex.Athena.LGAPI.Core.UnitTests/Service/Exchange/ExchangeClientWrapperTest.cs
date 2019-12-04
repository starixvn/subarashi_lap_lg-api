using System;
using CitiXchangeLibrary;
using Fanex.Athena.LGAPI.Core.Service.Exchange;
using Fanex.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Fanex.Athena.LGAPI.Core.UnitTests.Service.Exchange
{
    [TestFixture]
    public class ExchangeClientWrapperTest
    {
        private ICitiXchangeClient _apiClient;

        private ILogger _logger;

        private const string Environment = "DEV";
        private const int CustId = 1;

        [SetUp]
        public void SetUp()
        {
            _apiClient = Substitute.For<ICitiXchangeClient>();
            _logger = Substitute.For<ILogger>();
        }

        [TearDown]
        public void TearDown()
        {
            _apiClient = null;
            _logger = null;
        }

        [Test]
        public void GetCommission_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var expectedResult = new GetProfile();
            _apiClient.getProfile(CustId, Enviro.POC).Returns(expectedResult);

            var wrapper = new ExchangeClientWrapper(_apiClient, _logger);

            // Action
            var result = wrapper.GetCommission(CustId, Environment);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetCommission_ThrowException_LogCorrectInfo()
        {
            // Arrange
            const string ErrMsg = "GetCommission - ThrowException";
            var expectedResult = new GetProfile();
            _apiClient.getProfile(CustId, Enviro.POC).Returns(x => throw new Exception(ErrMsg));

            var wrapper = new ExchangeClientWrapper(_apiClient, _logger);

            // Action
            var result = wrapper.GetCommission(CustId, Environment);

            // Assert
            Assert.That(result, Is.Null);
            _logger.Received(1).Error(Arg.Any<string>(), Arg.Any<Exception>());
        }

        [Test]
        public void UpdateCommission_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            const double NewCommisison = 0.5;
            var expectedResult = new CommissionInfo();
            _apiClient.updateCommission(CustId, NewCommisison, Enviro.POC).Returns(expectedResult);

            var wrapper = new ExchangeClientWrapper(_apiClient, _logger);

            // Action
            var result = wrapper.UpdateCommission(CustId, NewCommisison, Environment);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void UpdateCommission_ThrowException_LogCorrectInfo()
        {
            // Arrange
            const string ErrMsg = "UpdateCommission - ThrowException";
            const double NewCommisison = 0.5;
            var expectedResult = new CommissionInfo();
            _apiClient.updateCommission(CustId, NewCommisison, Enviro.POC)
                .Returns(x => throw new Exception(ErrMsg));

            var wrapper = new ExchangeClientWrapper(_apiClient, _logger);

            // Action
            var result = wrapper.UpdateCommission(CustId, NewCommisison, Environment);

            // Assert
            Assert.That(result, Is.Null);
            _logger.Received(1).Error(Arg.Any<string>(), Arg.Any<Exception>());
        }
    }
}