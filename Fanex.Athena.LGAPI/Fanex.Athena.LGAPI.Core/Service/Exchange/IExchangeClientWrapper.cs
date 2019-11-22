using CitiXchangeLibrary;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public interface IExchangeClientWrapper
    {
        GetProfile GetCommission(int custId, string env);

        CommissionInfo UpdateCommission(int custId, double newCommission, string env);
    }
}