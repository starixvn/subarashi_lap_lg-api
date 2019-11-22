using System;
using System.Collections.Generic;
using CitiXchangeLibrary;
using Fanex.Athena.LGAPI.Core.Utility;
using Fanex.Logging;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public class ExchangeClientWrapper : IExchangeClientWrapper
    {
        private static readonly IDictionary<string, Enviro> EnvironmentMap =
            new Dictionary<string, Enviro>(StringComparer.OrdinalIgnoreCase)
        {
            { "PRO", Enviro.PRD },
            { "UAT", Enviro.UAT },
            { "SIT", Enviro.SIT },
            { "LOCAL", Enviro.POC },
            { "DEV", Enviro.POC }
        };

        private readonly ICitiXchangeClient _apiClient;

        private readonly ILogger _logger;

        public ExchangeClientWrapper(ICitiXchangeClient apiClient, ILogger logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public GetProfile GetCommission(int custId, string env)
        {
            GetProfile result = null;
            try
            {
                result = _apiClient.getProfile(custId, EnvironmentMap[env]);
            }
            catch (Exception exception)
            {
                var message = $"Exchange Api getProfile (custid: {custId}): " + LogMessageHelper.GenerateMessage(exception);
                _logger.Error(message, exception);
            }

            return result;
        }

        public CommissionInfo UpdateCommission(int custId, double newCommission, string env)
        {
            CommissionInfo result = null;
            try
            {
                result = _apiClient.updateCommission(custId, newCommission, EnvironmentMap[env]);
            }
            catch (Exception exception)
            {
                var message = $"Exchange Api updateCommission (custid: {custId}, value: {newCommission}):\n" + LogMessageHelper.GenerateMessage(exception);
                _logger.Error(message, exception);
            }

            return result;
        }
    }
}