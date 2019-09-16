using System.Collections.Generic;

namespace Fanex.Athena.LGAPI.SSAS.Services
{
    public interface ICubeService
    {
        IEnumerable<TResult> Query<TResult>(string query, IDictionary<string, object> parameters)
           where TResult : new();
    }
}