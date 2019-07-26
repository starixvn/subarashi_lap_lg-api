using System.Web.Mvc;
using Fanex.Athena.LGAPI.Core.Attributes;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.RNGCasino;
using Fanex.Athena.LGAPI.Core.RNGCasino.Service;
using Fanex.Athena.LGAPI.Core.Service.RNGCasino;

namespace Fanex.Athena.LGAPI.Core.Controllers
{
    [CustomAuth]
    [HandleJsonException]
    public class RNGCasinoController : Controller
    {
        private readonly ICasinoSettingService _casinoSetting;

        public RNGCasinoController(ICasinoSettingService casinoSettingService)
        {
            _casinoSetting = casinoSettingService ?? new CasinoSettingService(
                Logging.Logger.Log,
                new CustomerApiWrapper(),
                new BasicInfoApiWrapper());
        }

        [HttpPost]
        public ActionResult RngCasinoSettings(RngCasinoSettingMetaModel model)
        {
            var data = _casinoSetting.GetRngCasinoSettings(model.CustId, model.CurrencyID, model.Language);
            return Json(new { Data = data });
        }

        [HttpPost]
        public ActionResult UpdateSetting(RngCasinoUpdateSettingMetaModel model)
        {
            var data = _casinoSetting.UpdateSetting(model);
            return Json(new { Data = data });
        }
    }
}