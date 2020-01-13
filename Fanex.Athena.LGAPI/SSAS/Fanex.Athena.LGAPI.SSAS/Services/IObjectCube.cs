using System.Collections.Generic;

namespace Fanex.Athena.LGAPI.SSAS.Services
{
    public interface IObjectCube
    {
        IEnumerable<IDictionary<string, object>> Query(string contentQuery);

        IEnumerable<IEnumerable<IDictionary<string, object>>> QueryMultiple(IEnumerable<string> contentQueries);
    }
}