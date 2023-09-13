using Fanex.Athena.LGAPI.Core.Model.DTO.Exchange;
using System.Collections.Generic;

namespace Fanex.Athena.LGAPI.Core.Service.Exchange
{
    public class ExchangeSetting : IExchangeSetting
    {
        public IEnumerable<ExchangeLicenseeMapping> ExchangeLicenseeMappings { get; set; }
    }
}
