using System.Threading.Tasks;
using System.Web.Mvc;
using AutoFixture;
using Fanex.Athena.LGAPI.Core.Controllers;
using Fanex.Athena.LGAPI.Core.Model.DTO.Exchange;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;
using Fanex.Athena.LGAPI.Core.Service.Exchange;
using NSubstitute;
using NUnit.Framework;

namespace Fanex.Athena.LGAPI.Core.UnitTests.Controllers
{
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