namespace Fanex.Athena.LGAPI.Controllers
{
    using System.Web.Mvc;

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