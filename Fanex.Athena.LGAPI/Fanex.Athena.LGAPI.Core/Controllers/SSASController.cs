using Fanex.Athena.LGAPI.Core.Attributes;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.SSAS;
using Fanex.Athena.LGAPI.SSAS.Services;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Fanex.Athena.LGAPI.Core.Controllers
{
    [CustomAuth]
    [HandleJsonException]
    public class SSASController : Controller
    {
        [HttpPost]
        public async Task<ActionResult> Query(QueryMetaModel model)
        {
            var objCube = new ObjectCube(model.ConnectionKey);
            var result = await Task.Run(() => objCube.Query(model.Query));

            return Json(result);
        }

        [HttpPost]
        public async Task<ActionResult> QueryMultiple(QueryMultipleMetaModel model)
        {
            var objCube = new ObjectCube(model.ConnectionKey);
            var result = await Task.Run(() => objCube.QueryMultiple(model.Queries));

            return Json(result);
        }
    }
}