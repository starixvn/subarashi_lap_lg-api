namespace Fanex.Athena.LGAPI.Core.UnitTests.Service
{
    using System;
    using Fanex.Athena.LGAPI.Core.Service.RNGCasino;
    using Fanex.Athena.LGAPI.Core.UnitTests.HelperTest;
    using Fanex.Logging;

    using NSubstitute;

    using NUnit.Framework;

    [TestFixture]
    public class CasinoCustomerServiceTest
    {
        private ILogger _logger;

        private ICustomerApiWrapper _customerApiWrapper;

        private CasinoCustomerService _casinoCustomerService;

        [SetUp]
        public void SetUp()
        {
            _logger = Substitute.For<ILogger>();
            _customerApiWrapper = Substitute.For<ICustomerApiWrapper>();

            _casinoCustomerService = new CasinoCustomerService(_logger, _customerApiWrapper);
        }

        [TearDown]
        public void TearDown()
        {
            _customerApiWrapper = null;
        }

        [Test]
        public void UpdateSuspend_WhenCall_CallApiCorrectly()
        {
            // Arrange
            var metaModel = FakeHelper.Create<CustomerStatusMetaModel>();

            // Action
            _casinoCustomerService.UpdateSuspend(metaModel);

            // Assert
            _customerApiWrapper.Received().UpdateCustSuspend(metaModel);
        }

        [Test]
        public void UpdateSuspend_ApiThrowException_ReturnFalse()
        {
            // Arrange
            _customerApiWrapper.UpdateCustSuspend(Arg.Any<CustomerStatusMetaModel>()).ReturnsForAnyArgs(x =>
            {
                throw new Exception();
            });

            // Action
            var result = _casinoCustomerService.UpdateSuspend(null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void UpdateClosed_WhenCall_CallApiCorrectly()
        {
            // Arrange
            var metaModel = FakeHelper.Create<CustomerStatusMetaModel>();

            // Action
            _casinoCustomerService.UpdateClosed(metaModel);

            // Assert
            _customerApiWrapper.Received().UpdateCustClosed(metaModel);
        }

        [Test]
        public void UpdateClosed_ApiThrowException_ReturnFalse()
        {
            // Arrange
            _customerApiWrapper.UpdateCustClosed(Arg.Any<CustomerStatusMetaModel>()).ReturnsForAnyArgs(x =>
            {
                throw new Exception();
            });

            // Action
            var result = _casinoCustomerService.UpdateClosed(null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void UpdateCommission_WhenCall_CallApiCorrectly()
        {
            // Arrange
            var metaModel = FakeHelper.Create<RNGCasinoUpdateMemberComMetaModel>();

            _customerApiWrapper.UpdateComm(metaModel).Returns(true);

            // Action
            _casinoCustomerService.UpdateCommission(metaModel);

            // Assert
            _customerApiWrapper.Received().UpdateComm(metaModel);
        }

        [Test]
        public void UpdateCommission_CallApi_UpdateFail_LogError()
        {
            // Arrange
            var metaModel = FakeHelper.Create<RNGCasinoUpdateMemberComMetaModel>();

            _customerApiWrapper.UpdateComm(metaModel).Returns(false);

            // Action
            _casinoCustomerService.UpdateCommission(metaModel);

            // Assert
            _logger.Received(1).Error(Arg.Is<string>(x => x.Contains(string.Join(",", metaModel.CustomerId))));
        }

        [Test]
        public void UpdateCommission_ApiThrowException_LogException()
        {
            // Arrange
            var exception = new Exception("Hello");
            var metaModel = FakeHelper.Create<RNGCasinoUpdateMemberComMetaModel>();

            _customerApiWrapper.UpdateComm(Arg.Any<RNGCasinoUpdateMemberComMetaModel>()).ReturnsForAnyArgs(x =>
            {
                throw exception;
            });

            // Action
            _casinoCustomerService.UpdateCommission(metaModel);

            // Assert
            _logger.ReceivedWithAnyArgs().Error(exception.Message, exception);
        }
    }
}