using System;
using System.Collections.Generic;
using System.Data;

namespace Fanex.Athena.LGAPI.SSAS.Services
{
    public interface IObjectCube : IDisposable
    {
        IEnumerable<IDictionary<string, object>> Query(string query, IDictionary<string, object> parameters = null);

        IDataReader ExecuteReader(string query);
    }
}