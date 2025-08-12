namespace Fanex.Athena.LGAPI.Core.UnitTests.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using AutoFixture;
    using Core.Controllers;
    using Core.Service.Exchange;
    using Model.DTO.Exchange;
    using Model.MetalModel.Exchange;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class ExchangeBetHistoryControllerTest
    {
        private static Fixture _fixture = new Fixture();

        private IExchangeBetHistoryService _exchangeService;

        [SetUp]
        public void SetUp()
        {
            _exchangeService = Substitute.For<IExchangeBetHistoryService>();
        }

        [TearDown]
        public void TearDown()
        {
            _exchangeService = null;
        }

        [Test]
        public async Task Index_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<ExchangeGettingVoidedMetaModel>();
            var expectedResult = new BetHistoryResult();

            _exchangeService.GetVoidedData(metaModel).Returns(expectedResult);

            using (var controller = new ExchangeBetHistoryController(_exchangeService))
            {
                // Action
                var jsonResult = await controller.Index(metaModel) as JsonResult;
                var result = jsonResult.Data;

                // Assert
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }
    }
}