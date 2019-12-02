using System.Web.Mvc;
using Fanex.Athena.LGAPI.Core.Attributes;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;
using Fanex.Athena.LGAPI.Core.Service.Exchange;

namespace Fanex.Athena.LGAPI.Core.Controllers
{
    [CustomAuth]
    [HandleJsonException]
    public class ExchangeBetHistoryController : Controller
    {
        private readonly IExchangeBetHistoryService _exchangeService;

        public ExchangeBetHistoryController(IExchangeBetHistoryService exchangeService)
        {
            _exchangeService = exchangeService;
        }

        [HttpPost]
        public ActionResult Index(ExchangeGettingVoidedMetaModel metaModel)
        {
            return Json(_exchangeService.GetVoidedData(metaModel));
        }
    }
}