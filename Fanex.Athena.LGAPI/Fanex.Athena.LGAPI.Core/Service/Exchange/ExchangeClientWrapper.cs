using System;
using CitiXchangeLibrary;
using Fanex.Athena.LGAPI.Core.Constants;
using Fanex.Athena.LGAPI.Core.Utility;
using Fanex.Logging;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public class ExchangeClientWrapper : IExchangeClientWrapper
    {
        private readonly ICitiXchangeClient _apiClient;

        private readonly ILogger _logger;

        public ExchangeClientWrapper(ICitiXchangeClient apiClient, ILogger logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public GetProfile GetCommission(int custId, string env, int groupMode)
        {
            GetProfile result = null;
            try
            {
                _logger.Info($"{custId} - {env} - {groupMode}");
                result = _apiClient.getProfile(custId, ExchangeEnvironment.EnvironmentMap[env], groupMode);
            }
            catch (Exception exception)
            {
                var message = $"Exchange Api getProfile (custid: {custId}): " + LogMessageHelper.GenerateMessage(exception);
                _logger.Error(message, exception);
            }

            return result;
        }

        public CommissionInfo UpdateCommission(int custId, double newCommission, string env, int groupMode)
        {
            CommissionInfo result = null;
            var message = $"Exchange Api updateCommission (custid: {custId}, value: {newCommission})";

            try
            {
                _logger.Info(message);
                result = _apiClient.updateCommission(custId, newCommission, ExchangeEnvironment.EnvironmentMap[env], groupMode);
            }
            catch (Exception exception)
            {
                _logger.Error($"{message}\n{LogMessageHelper.GenerateMessage(exception)}", exception);
            }

            return result;
        }
    }
}
