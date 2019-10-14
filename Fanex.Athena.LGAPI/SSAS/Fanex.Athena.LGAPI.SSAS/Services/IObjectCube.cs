using System;
using System.Collections.Generic;
using System.Data;

namespace Fanex.Athena.LGAPI.SSAS.Services
{
    public interface IObjectCube
    {
        IEnumerable<IDictionary<string, object>> Query(string queryContent, IDictionary<string, object> parameters = null);

        IDataReader ExecuteReader(string queryContent);
    }
}