using Fanex.Athena.LGAPI.Core.Attributes;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.SSAS;
using Fanex.Athena.LGAPI.SSAS.Services;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Fanex.Athena.LGAPI.Core.Controllers
{
    [HandleJsonException]
    public class SSASController : Controller
    {
        [HttpPost]
        public ActionResult Query(QueryInfoMetalModel model)
        {
            IEnumerable<IDictionary<string, object>> result = null;
            using (var objCube = new ObjectCube(model.ConnectionKey))
            {
                result = objCube.Query(model.Query);

            }

            return Json(result);
        }
    }
}