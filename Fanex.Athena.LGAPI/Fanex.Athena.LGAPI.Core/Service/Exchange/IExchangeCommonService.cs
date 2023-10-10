using System.Threading.Tasks;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public interface IExchangeCommonService
    {
        Task<bool> IsUnderMaintenance();
    }
}