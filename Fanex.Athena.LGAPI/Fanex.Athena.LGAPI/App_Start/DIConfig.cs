using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Fanex.Athena.LGAPI.Core.Model.Configuration;
using Fanex.Athena.LGAPI.Core.Model.DTO.Exchange;
using Fanex.Athena.LGAPI.Core.Service;
using Fanex.Athena.LGAPI.Core.Service.Exchange;
using Fanex.Athena.LGAPI.Core.Utility;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Logging;
using Fanex.ThirdParty.ExchangeProduct;
using Fanex.ThirdParty.ExchangeProduct.Enums;
using Fanex.ThirdParty.ExchangeProduct.HttpClients;
using Fanex.ThirdParty.ExchangeProduct.Model;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;

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

            RegisterCachingService(builder);
            RegisterThirdpartyServices(builder);
            RegisterCircuitBreaker(builder);
            RegisterSingleton(builder);

            RegisterExchangeSetting(builder);

            builder.RegisterControllers(assemblies);
            builder.RegisterApiControllers(assemblies);

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        private static void RegisterExchangeSetting(ContainerBuilder builder)
        {
            builder.Register(x => new ExchangeSetting { ExchangeLicenseeMappings = GetExchangeLicenseeMapping()})
                .As<IExchangeSetting>()
                .SingleInstance();
        }

        private static void RegisterThirdpartyServices(ContainerBuilder builder)
        {
            builder.RegisterInstance(Logger.Log).As<ILogger>();

            builder.Register(x => new ExchangeHttpClient(new HttpClient(), new ExchangeOption { BaseAddress = ConfigurationManager.AppSettings["ExchangeProductApiHost"] }))
                .As<IExchangeHttpClient>()
                .SingleInstance();
        }

        private static void RegisterCircuitBreaker(ContainerBuilder builder)
        {
            const int handledEventsAllowedBeforeBreaking = 2;
            const int durationOfBreakInSecond = 2;

            builder.Register(x => Policy
                .HandleResult<BetHistory>(r => r.status != CitiStatus.Success && r.status != 0).Or<Exception>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking,
                    durationOfBreak: TimeSpan.FromSeconds(durationOfBreakInSecond),
                    onBreak: OnBreak,
                    onReset: () => { }))
                .As<IAsyncPolicy<BetHistory>>()
                .SingleInstance();
        }

        private static void OnBreak(DelegateResult<BetHistory> delegateResult, TimeSpan timeSpan)
        {
            var messageBuilder = new StringBuilder("Break on Exchange BetHistory");
            if (delegateResult.Exception != null)
            {
                messageBuilder.AppendLine(LogMessageHelper.GenerateMessage(delegateResult.Exception));
            }

            if (delegateResult.Result != null)
            {
                var result = delegateResult.Result;
                var resultInfo = $"\r\nResult: Status={result.status.ToString()} - PageIndex={result.currentPage}";
                messageBuilder.AppendLine(resultInfo);
            }

            LogService.Info(messageBuilder.ToString());
        }

        private static void RegisterCachingService(ContainerBuilder builder)
        {
            builder.Register(x => new RedisCache(new RedisCacheOptions
            {
                ConnectionString = ConfigurationManager.AppSettings["RedisConnectionString"],
                ApplicationName = "LAP.LGAPI"
            })).As<ICache>().SingleInstance();

            builder.RegisterType<CacheService>().As<ICacheService>().SingleInstance();
        }

        private static void RegisterSingleton(ContainerBuilder builder)
        {
            builder.Register(x => new PortalService(
                new PortalOptions(
                    ConfigurationManager.AppSettings["Portal_Address"],
                    ConfigurationManager.AppSettings["Portal_Proxy"],
                    ConfigurationManager.AppSettings["Portal_AccessKey"]))
                ).As<IPortalService>().SingleInstance();
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

        private static IEnumerable<ExchangeLicenseeMapping> GetExchangeLicenseeMapping()
        {
            var FilePath = $"~/App_Data/ExchangeLicenseeMapping.{ConfigurationManager.AppSettings["Evr"]}.json";
            using (var reader = new StreamReader(HostingEnvironment.MapPath(FilePath)))
            {
                var json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<IEnumerable<ExchangeLicenseeMapping>>(json);
            }
        }
    }
}