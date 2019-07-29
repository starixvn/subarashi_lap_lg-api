using System;
using System.Text;
using Fanex.Athena.LGAPI.Core.Utility;
using Fanex.Logging;

namespace Fanex.Athena.LGAPI.Core.Service.RNGCasino
{
    public class CasinoCustomerService : ICasinoCustomerService
    {
        private readonly ILogger _logger;

        private readonly ICustomerApiWrapper _customerApiWrapper;

        public CasinoCustomerService(ILogger logger, ICustomerApiWrapper customerApiWrapper)
        {
            _logger = logger;
            _customerApiWrapper = customerApiWrapper;
        }

        public bool UpdateSuspend(CustomerStatusMetaModel metaModel)
        {
            try
            {
                return _customerApiWrapper.UpdateCustSuspend(metaModel);
            }
            catch (Exception e)
            {
                var message = new StringBuilder(LogMessageHelper.GenerateMessage(e));
                message.AppendLine("UpdateSuspend:" + $"Error when call casino api UpdateCustSuspend. Message:{e.Message} casino api service");
                _logger.Error(message.ToString(), e);
            }
            return false;
        }

        public bool UpdateClosed(CustomerStatusMetaModel metaModel)
        {
            try
            {
                return _customerApiWrapper.UpdateCustClosed(
                         metaModel);
            }
            catch (Exception e)
            {
                var message = new StringBuilder(LogMessageHelper.GenerateMessage(e));
                message.AppendLine("Error when call casino api UpdateCustClosed. Message:{e.Message} casino api service");
                _logger.Error(message.ToString(), e);
            }

            return false;

        }

        public bool UpdateCommission(RNGCasinoUpdateMemberComMetaModel metaModel)
        {
            const string API_ERROR_MSG = "Found errors when calling casino API to update customer commission.\n customers=";
            const string AdditionErrorMsgPattern = "\n IP={0},CustomerId={1},ModifyBy={2},ModifyBy={3},Site={4}";
            var additionalErrorMsg = string.Format(
                AdditionErrorMsgPattern,
                metaModel.FromIp,
                string.Join(",", metaModel.CustomerId),
                metaModel.ModifyBy,
                metaModel.ModifyUserName,
                metaModel.SiteName);
            var success = false;
            try
            {
                success = _customerApiWrapper.UpdateComm(metaModel);
                if (!success)
                {
                    _logger.Error(API_ERROR_MSG + string.Join(",", metaModel.CustomerId) + additionalErrorMsg);
                }
            }
            catch (Exception exception)
            {
                var message = new StringBuilder(LogMessageHelper.GenerateMessage(exception));
                message.Append("\nUpdateCommission:" + additionalErrorMsg);
                _logger.Error(message.ToString(), exception);
            }

            return success;
        }
    }
}