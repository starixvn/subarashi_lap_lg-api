namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Caching;
    using Infrastructure;
    using Logging;
    using Model.DTO.Exchange;
    using ThirdParty.ExchangeProduct.HttpClients;
    using Utility;

    public class ExchangeCommonService : IExchangeCommonService
    {
        protected readonly IExchangeHttpClient _apiClient;

        protected readonly ILogger _logger;

        protected readonly ICacheService _cacheService;

        protected readonly IDbFactory _dbFactory;

        private readonly int ExpirationTimeInMinute = 2;

        public ExchangeCommonService(
            IExchangeHttpClient apiClient,
            ILogger logger,
            ICacheService cacheService,
            IDbFactory dbFactory)
        {
            _apiClient = apiClient;
            _logger = logger;
            _cacheService = cacheService;
            _dbFactory = dbFactory;
        }

        public async Task<bool> IsUnderMaintenance()
        {
            const string cacheKey = "Exchange_Is_Under_Maintenance";
            var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(ExpirationTimeInMinute));

            if (_cacheService.TryGet(cacheKey, out bool result))
            {
                return result;
            }

            try
            {
                result = await _apiClient.IsUnderMaintenance();
                _cacheService.Set(cacheKey, result, cachePolicy);

                return result;
            }
            catch (Exception exception)
            {
                const string message = "Exchange API Check UM";

                _logger.Error($"{message}\n{LogMessageHelper.GenerateMessage(exception)}", exception);
                _cacheService.Set(cacheKey, true, cachePolicy);

                return true;
            }
        }

        public IEnumerable<ExchangeLicenseeMapping> ExchangeLicenseeMappings()
        {
            const string cacheKey = "Exchange_Licensee_Mappings";
            var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

            if (_cacheService.TryGet(cacheKey, out IEnumerable<ExchangeLicenseeMapping> result))
            {
                return result;
            }

            const string SpName = "Nap_LM_LicVendorInfo_Select";
            var uspParams = new { pageIndex = 1, pageSize = 1000 };
            using (var db = _dbFactory.GetObjectDb(SpName))
            {
                result = db.Query<ExchangeLicenseeMapping>(uspParams);
                _cacheService.Set(cacheKey, result, cachePolicy);
                return result;
            }
        }
    }
}