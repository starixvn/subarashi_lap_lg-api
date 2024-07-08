using System;
using System.Threading.Tasks;
using Fanex.Athena.LGAPI.Core.Infrastructure;
using Fanex.Athena.LGAPI.Core.Utility;
using Fanex.Caching;
using Fanex.Logging;
using Fanex.ThirdParty.ExchangeProduct.HttpClients;
using Fanex.ThirdParty.ExchangeProduct.Model;
using Fanex.ThirdParty.ExchangeProduct.Params;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public class ExchangeClientWrapper : ExchangeCommonService, IExchangeClientWrapper
    {
        public ExchangeClientWrapper(
            IExchangeHttpClient apiClient,
            ILogger logger,
            ICacheService cacheService,
            IDbFactory dbFactory)
            : base(apiClient, logger, cacheService, dbFactory) { }

        public async Task<GetProfile> GetCommission(int custId, int groupMode)
        {
            GetProfile result = null;
            try
            {
                var response = await _apiClient.GetProfileAsync(new GetProfileParam
                {
                    CustId = custId,
                    GroupMode = groupMode
                });
                result = response.Result;
            }
            catch (Exception exception)
            {
                var message = $"Exchange Api getProfile (custid: {custId}): " + LogMessageHelper.GenerateMessage(exception);
                _logger.Error(message, exception);
            }

            return result;
        }

        public async Task<CommissionInfo> UpdateCommission(int custId, double newCommission, int groupMode)
        {
            CommissionInfo result = null;
            var message = $"Exchange Api updateCommission (custid: {custId}, value: {newCommission})";

            try
            {
                var response = await _apiClient.UpdateCommissionAsync(new UpdateCommissionParam
                {
                    CustId = custId,
                    Commission = newCommission,
                    GroupMode = groupMode
                });
                result = response.Result;
            }
            catch (Exception exception)
            {
                _logger.Error($"{message}\n{LogMessageHelper.GenerateMessage(exception)}", exception);
            }

            return result;
        }
    }
}