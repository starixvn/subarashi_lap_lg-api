using System.Threading.Tasks;
using System.Web.Mvc;
using AutoFixture;
using Fanex.Athena.LGAPI.Core.Controllers;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;
using Fanex.Athena.LGAPI.Core.Service.Exchange;
using Fanex.ThirdParty.ExchangeProduct.Model;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace Fanex.Athena.LGAPI.Core.UnitTests.Controllers
{
    [TestFixture]
    public class ExchangeControllerTest
    {
        private readonly Fixture _fixture = new Fixture();

        private IExchangeClientWrapper _exchangeWrapper;

        [SetUp]
        public void SetUp()
        {
            _exchangeWrapper = Substitute.For<IExchangeClientWrapper>();
        }

        [TearDown]
        public void TearDown()
        {
            _exchangeWrapper = null;
        }

        [Test]
        public async Task GetCommission_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<ExchangeBaseMetaModel>();
            var expectedProfile = _fixture.Create<GetProfile>();

            _exchangeWrapper.GetCommission(metaModel.CustId, metaModel.GroupMode).Returns(expectedProfile);
            var expectedResult = JsonConvert.SerializeObject(new { Data = expectedProfile });

            using (var controller = new ExchangeController(_exchangeWrapper))
            {
                // Action
                var jsonResult = await controller.GetCommission(metaModel) as JsonResult;
                var result = JsonConvert.SerializeObject(jsonResult.Data);

                // Assert
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        public async Task UpdateCommission_WhenCall_ReturnCorrectResult()
        {
            // Arrange
            var metaModel = _fixture.Create<ExchangeUpdatingCommissionMetaModel>();
            var commissionInfo = _fixture.Create<CommissionInfo>();

            _exchangeWrapper.UpdateCommission(metaModel.CustId, metaModel.Commission, metaModel.GroupMode)
                .Returns(commissionInfo);
            var expectedResult = JsonConvert.SerializeObject(new { Data = commissionInfo });

            using (var controller = new ExchangeController(_exchangeWrapper))
            {
                // Action
                var jsonResult = await controller.UpdateCommission(metaModel) as JsonResult;
                var result = JsonConvert.SerializeObject(jsonResult.Data);

                // Assert
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }
    }
}