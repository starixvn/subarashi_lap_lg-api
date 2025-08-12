namespace Fanex.Athena.LGAPI.Core.Attributes
{
    using System;
    using System.Net;
    using System.Web.Mvc;
    using Logging;
    using Utility;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class HandleJsonExceptionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Exception != null)
            {
                LogService.Error(
                    LogMessageHelper.GenerateMessage(filterContext.Exception));
                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
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