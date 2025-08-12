namespace Fanex.Athena.LGAPI.Core.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Attributes;
    using Model.MetalModel.Exchange;
    using Service.Exchange;

    [CustomAuth]
    [HandleJsonException]
    public class ExchangeController : Controller
    {
        private readonly IExchangeClientWrapper _exchangeClient;

        public ExchangeController(IExchangeClientWrapper exchangeClient)
        {
            _exchangeClient = exchangeClient;
        }

        [HttpPost]
        public async Task<ActionResult> GetCommission(ExchangeBaseMetaModel metaModel)
        {
            var result = await _exchangeClient.GetCommission(metaModel.CustId, metaModel.GroupMode);

            return Json(new { Data = result });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateCommission(ExchangeUpdatingCommissionMetaModel metaModel)
        {
            var result = await _exchangeClient.UpdateCommission(metaModel.CustId, metaModel.Commission, metaModel.GroupMode);

            return Json(new { Data = result });
        }

        [HttpPost]
        public async Task<ActionResult> IsUnderMaintenance()
        {
            var result = await _exchangeClient.IsUnderMaintenance();

            return Json(result);
        }
    }
}