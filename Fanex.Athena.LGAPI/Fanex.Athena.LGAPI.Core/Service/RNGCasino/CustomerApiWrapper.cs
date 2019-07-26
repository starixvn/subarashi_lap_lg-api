using System.Data;

using CasinoLibrary.SBAgent;

namespace Fanex.Athena.LGAPI.Core.Service.RNGCasino
{
    public class CustomerApiWrapper : ICustomerApiWrapper
    {
        private readonly Customer _customerApi;
        public CustomerApiWrapper()
        {
            _customerApi = new Customer();
        }

        public DataTable GetCustStakeLevel(int ibcCustId, int gameTypeId)
        {
            return _customerApi.GetCustStakeLevel(ibcCustId, gameTypeId);
        }

        public DataTable UpdateCustGameTypeStakeLevel(DataTable sourceTable)
        {
            return new ApiMethodOperationHandler<DataTable>().Handle<DataTable>(
                sourceTable,
                (obj) => _customerApi.UpdateCustGameTypeStakeLevel(obj));
        }
    }
}
