using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.LGAPI.Core.Model.DTO.Exchange;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public interface IExchangeCommonService
    {
        Task<bool> IsUnderMaintenance();

        IEnumerable<ExchangeLicenseeMapping> ExchangeLicenseeMappings();
    }
}