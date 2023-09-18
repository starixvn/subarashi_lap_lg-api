using Fanex.Athena.LGAPI.Core.Model.DTO.Exchange;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;
using System.Threading.Tasks;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public interface IExchangeBetHistoryService
    {
        Task<BetHistoryResult> GetVoidedData(ExchangeGettingVoidedMetaModel metaModel);
    }
}