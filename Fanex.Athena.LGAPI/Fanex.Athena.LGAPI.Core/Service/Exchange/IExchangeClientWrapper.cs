
using Fanex.ThirdParty.ExchangeProduct.Model;
using System.Threading.Tasks;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public interface IExchangeClientWrapper
    {
        Task<GetProfile> GetCommission(int custId, int groupMode);

        Task<CommissionInfo> UpdateCommission(int custId, double newCommission, int groupMode);
    }
}