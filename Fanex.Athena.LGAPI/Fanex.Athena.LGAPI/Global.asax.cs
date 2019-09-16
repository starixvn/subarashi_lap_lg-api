using System;
using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Fanex.Athena.LGAPI.Core.Utility;
using Fanex.Athena.LGAPI.SSAS.Configuration;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using SharpRaven;

namespace Fanex.Athena.LGAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            LogManager.SetDefaultLogCategory(ConfigurationManager.AppSettings["Fanex.Logging-LogCategory"])
                .Use(new SentryLogging(new SentryEngineOptions
                {
                    Dsn = new Dsn(ConfigurationManager.AppSettings["Sentry-Dsn"])
                }));

            CubeSettingProvider.UseTabularCube();

            DIConfig.Register();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        public void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
            var exception = Server.GetLastError();
            var isIgnore = exception is HttpAntiForgeryException
                          || exception is HttpException
                       || (exception.TargetSite.DeclaringType == typeof(ActionDescriptor) &&
                            exception.TargetSite.Name == "ExtractParameterFromDictionary");
            // Exception when passing wrong data type to action method
            if (!isIgnore)
            {
                LogService.Error(LogMessageHelper.GenerateMessage(exception), exception);
            }
        }
    }
}