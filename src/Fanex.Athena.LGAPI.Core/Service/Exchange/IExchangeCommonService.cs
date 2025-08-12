namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Model.DTO.Exchange;

    public interface IExchangeCommonService
    {
        Task<bool> IsUnderMaintenance();

        IEnumerable<ExchangeLicenseeMapping> ExchangeLicenseeMappings();
    }
}