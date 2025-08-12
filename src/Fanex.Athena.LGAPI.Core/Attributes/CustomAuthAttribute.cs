namespace Fanex.Athena.LGAPI.Core.Attributes
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Web.Mvc;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class CustomAuthAttribute : FilterAttribute, IAuthorizationFilter
    {
        private const string KEYNAME = "LGAPI_TOKEN_KEY";

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext.RequestContext.HttpContext.Request;
            var token = request.Headers.Get(KEYNAME);
            if (token != ConfigurationManager.AppSettings["LGAPITokenValue"])
            {
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        Error = "Invalid token"
                    }
                };
            }
        }
    }
}