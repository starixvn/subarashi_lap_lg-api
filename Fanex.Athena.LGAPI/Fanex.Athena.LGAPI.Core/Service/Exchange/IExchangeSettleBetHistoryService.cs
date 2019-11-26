using Fanex.Athena.LGAPI.Core.Model.DTO.Exchange;
using Fanex.Athena.LGAPI.Core.Model.MetalModel.Exchange;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public interface IExchangeSettleBetHistoryService
    {
        SettleBetHistoryResult GetVoidedData(ExchangeGettingVoidedMetaModel metaModel);
    }
}