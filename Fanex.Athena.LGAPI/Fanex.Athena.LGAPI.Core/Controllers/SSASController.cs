using Fanex.Athena.LGAPI.Core.Attributes;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.SSAS;
using Fanex.Athena.LGAPI.SSAS.Services;
using System.Web.Mvc;

namespace Fanex.Athena.LGAPI.Core.Controllers
{
    [CustomAuth]
    [HandleJsonException]
    public class SSASController : Controller
    {
        [HttpPost]
        public ActionResult Query(QueryInfoMetalModel model)
        {
            var objCube = new ObjectCube(model.ConnectionKey);
            var result = objCube.Query(model.Query, model.Parameters);

            return Json(result);
        }
    }
}