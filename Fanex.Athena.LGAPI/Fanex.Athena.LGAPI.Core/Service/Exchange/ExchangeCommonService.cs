using System;
using System.Threading.Tasks;
using Fanex.Athena.LGAPI.Core.Utility;
using Fanex.Caching;
using Fanex.Logging;
using Fanex.ThirdParty.ExchangeProduct.HttpClients;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public class ExchangeCommonService : IExchangeCommonService
    {
        protected readonly IExchangeHttpClient _apiClient;

        protected readonly ILogger _logger;

        protected readonly ICacheService _cacheService;

        private readonly int ExpirationTimeInMinute = 2;

        public ExchangeCommonService(IExchangeHttpClient apiClient, ILogger logger, ICacheService cacheService)
        {
            _apiClient = apiClient;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<bool> IsUnderMaintenance()
        {
            const string cacheKey = "Exchange_Is_Under_Maintenance";
            var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(ExpirationTimeInMinute));
            bool result = false;

            if (_cacheService.TryGet<bool>(cacheKey, out result))
            {
                return result;
            }

            try
            {
                result = await _apiClient.IsUnderMaintenance();
                _cacheService.Set<bool>(cacheKey, result, cachePolicy);

                return result;
            }
            catch (Exception exception)
            {
                const string message = "Exchange API Check UM";

                _logger.Error($"{message}\n{LogMessageHelper.GenerateMessage(exception)}", exception);
                _cacheService.Set<bool>(cacheKey, true, cachePolicy);

                return true;
            }
        }
    }
}