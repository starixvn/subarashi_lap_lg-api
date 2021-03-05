using System.Web.Mvc;

namespace Fanex.Athena.LGAPI.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public string Index()
        {
            const string result = "LG API is running";
            return result;
        }
    }
}