using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.LGAPI.SSAS.Bridge.Services
{
    public interface ISsasBridgeService
    {
        Task<IEnumerable<TResult>> QueryAsync<TResult>(string query, string bridgeUrl = "ssas/query")
                        where TResult : new();
    }
}