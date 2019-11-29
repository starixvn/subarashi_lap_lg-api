using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using CitiXchangeLibrary;
using Fanex.Logging;
using Polly;

namespace Fanex.Athena.LGAPI
{
    public static class DIConfig
    {
        public static void Register()
        {
            var builder = new ContainerBuilder();
            var config = GlobalConfiguration.Configuration;
            var assemblies = GetAssemblies();
            builder.RegisterAssemblyTypes(assemblies).AsImplementedInterfaces();

            builder.RegisterFilterProvider();
            builder.RegisterWebApiFilterProvider(config);

            RegisterThirdpartyServices(builder);
            RegisterCircuitBreaker(builder);

            builder.RegisterControllers(assemblies);
            builder.RegisterApiControllers(assemblies);

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        private static void RegisterThirdpartyServices(ContainerBuilder builder)
        {
            builder.RegisterInstance(Logger.Log).As<Logging.ILogger>();

            builder.Register(x => (ICitiXchangeClient)Activator.GetObject(typeof(ICitiXchangeClient), ConfigurationManager.AppSettings["ExchangeProductApiHost"]))
                .As<ICitiXchangeClient>()
                .SingleInstance()
                .ExternallyOwned();
        }

        private static void RegisterCircuitBreaker(ContainerBuilder builder)
        {
            builder.Register(x => Policy
                .HandleResult<BetHistory>(r => r.status != CitiStatus.Success).Or<Exception>()
                .CircuitBreaker(handledEventsAllowedBeforeBreaking: 2, durationOfBreak: TimeSpan.FromSeconds(10)))
                .SingleInstance();
        }

        private static Assembly[] GetAssemblies()
        {
            Assembly[] assemblies = null;
            try
            {
                assemblies = BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray();
            }
            catch (Exception)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(i => !i.IsDynamic).ToArray();
            }

            return assemblies.Where(x => x.FullName.StartsWith("Fanex.Athena.LGAPI", StringComparison.InvariantCulture))
                .ToArray();
        }
    }
}