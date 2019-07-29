using System.Data;

using CasinoLibrary.SBAgent;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.RNGCasino;

namespace Fanex.Athena.LGAPI.Core.Service.RNGCasino
{
    public class CustomerApiWrapper : ICustomerApiWrapper
    {
        private const int SystemId = 4;

        private readonly Customer _customerApi;
        public CustomerApiWrapper()
        {
            _customerApi = new Customer();
        }

        public DataTable GetCustStakeLevel(int ibcCustId, int gameTypeId)
        {
            return _customerApi.GetCustStakeLevel(ibcCustId, gameTypeId);
        }

        public bool UpdateComm(RNGCasinoUpdateMemberComMetaModel metaModel)
        {
            return new ApiMethodOperationHandler<bool>().Handle<RNGCasinoUpdateMemberComMetaModel>(
                metaModel,
                (obj) => _customerApi.UpdateComm(obj.CustomerId, obj.Comm, obj.FromIp, obj.ModifyBy, obj.ModifyUserName, SystemId));
        }

        public bool UpdateCustClosed(CustomerStatusMetaModel metaModel)
        {
            return new ApiMethodOperationHandler<bool>().Handle<CustomerStatusMetaModel>(
                metaModel,
                (obj) => _customerApi.UpdateCustClosed(obj.CustId, obj.IsOn, obj.Ip, obj.ModifyById, obj.ModifyByUsername, SystemId));
        }

        public bool UpdateCustEnableCasino(BetSettingCasinoStatusMetaModel metaModel)
        {
            return new ApiMethodOperationHandler<bool>().Handle(
                metaModel,
                (obj) => _customerApi.UpdateCustEnableCasino(obj.CustId, obj.Status == 0, false, obj.Ip, obj.UserId, obj.UserName, SystemId));
        }

        public DataTable UpdateCustGameTypeStakeLevel(DataTable sourceTable)
        {
            return new ApiMethodOperationHandler<DataTable>().Handle<DataTable>(
                sourceTable,
                (obj) => _customerApi.UpdateCustGameTypeStakeLevel(obj));
        }

        public bool UpdateCustSuspend(CustomerStatusMetaModel metaModel)
        {
            return new ApiMethodOperationHandler<bool>().Handle(
                metaModel,
                (obj) => _customerApi.UpdateCustSuspend(obj.CustId, obj.IsOn, obj.Ip, obj.ModifyById, obj.ModifyByUsername, SystemId));
        }
    }
}
