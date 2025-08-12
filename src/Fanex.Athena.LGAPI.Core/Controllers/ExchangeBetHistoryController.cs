namespace Fanex.Athena.LGAPI.Core.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Attributes;
    using Model.MetalModel.Exchange;
    using Service.Exchange;

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
        public async Task<ActionResult> Index(ExchangeGettingVoidedMetaModel metaModel)
        {
            return Json(await _exchangeService.GetVoidedData(metaModel));
        }
    }
}