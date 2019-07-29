using System.Web.Mvc;
using Fanex.Athena.LGAPI.Core.Attributes;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.RNGCasino;
using Fanex.Athena.LGAPI.Core.RNGCasino.Service;
using Fanex.Athena.LGAPI.Core.Service.RNGCasino;

namespace Fanex.Athena.LGAPI.Core.Controllers
{
    [CustomAuth]
    [HandleJsonException]
    public class RNGCasinoCustomerController : Controller
    {
        private readonly ICasinoCustomerService _casinoCustomerService;
        public RNGCasinoCustomerController(ICasinoCustomerService casinoCustomerService)
        {
            _casinoCustomerService = casinoCustomerService ?? new CasinoCustomerService(
                Logging.Logger.Log,
                new CustomerApiWrapper());
        }

        [HttpPost]
        public ActionResult Suspend(CustomerStatusMetaModel model)
        {
            var data = _casinoCustomerService.UpdateSuspend(model);
            return Json(new { Data = data });
        }

        [HttpPost]
        public ActionResult Closed(CustomerStatusMetaModel model)
        {
            var data = _casinoCustomerService.UpdateClosed(model);
            return Json(new { Data = data });
        }

        [HttpPost]
        public ActionResult UpdateCommission(RNGCasinoUpdateMemberComMetaModel model)
        {
            var data = _casinoCustomerService.UpdateCommission(model);
            return Json(new { Data = data });
        }
        
    }
}