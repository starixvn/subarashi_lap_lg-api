using System.Web.Mvc;
using Fanex.Athena.LGAPI.Core.Attributes;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;
using Fanex.Athena.LGAPI.Core.Service.Exchange;

namespace Fanex.Athena.LGAPI.Core.Controllers
{
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
        public ActionResult GetCommission(ExchangeBaseMetaModel metaModel)
        {
            var result = _exchangeClient.GetCommission(metaModel.CustId, metaModel.Environment, 0);

            return Json(new { Data = result });
        }

        [HttpPost]
        public ActionResult UpdateCommission(ExchangeUpdatingCommissionMetaModel metaModel)
        {
            var result = _exchangeClient.UpdateCommission(metaModel.CustId, metaModel.Commission, metaModel.Environment, metaModel.GroupMode);

            return Json(new { Data = result });
        }
    }
}