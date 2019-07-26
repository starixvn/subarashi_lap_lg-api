using System;
using System.Web.Mvc;
using Fanex.Athena.LGAPI.Core.Utility;

namespace Fanex.Athena.LGAPI.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class HandleJsonExceptionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Exception != null)
            {
                Logging.LogService.Error(
                    LogMessageHelper.GenerateMessage(filterContext.Exception));
                filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        Error = filterContext.Exception.Message
                    }
                };
                filterContext.ExceptionHandled = true;
            }
        }
    }
}