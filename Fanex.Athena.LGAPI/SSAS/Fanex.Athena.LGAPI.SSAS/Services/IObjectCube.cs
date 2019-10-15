using System.Collections.Generic;

namespace Fanex.Athena.LGAPI.SSAS.Services
{
    public interface IObjectCube
    {
        IEnumerable<IDictionary<string, object>> Query(string queryContent, IDictionary<string, object> parameters = null);
    }
}