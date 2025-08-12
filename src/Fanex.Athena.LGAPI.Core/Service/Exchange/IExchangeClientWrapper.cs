namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    using System.Threading.Tasks;
    using ThirdParty.ExchangeProduct.Model;

    public interface IExchangeClientWrapper
    {
        Task<GetProfile> GetCommission(int custId, int groupMode);

        Task<CommissionInfo> UpdateCommission(int custId, double newCommission, int groupMode);

        Task<bool> IsUnderMaintenance();
    }
}