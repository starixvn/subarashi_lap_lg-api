namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    using System.Threading.Tasks;
    using Model.DTO.Exchange;
    using Model.MetalModel.Exchange;

    public interface IExchangeBetHistoryService
    {
        Task<BetHistoryResult> GetVoidedData(ExchangeGettingVoidedMetaModel metaModel);
    }
}