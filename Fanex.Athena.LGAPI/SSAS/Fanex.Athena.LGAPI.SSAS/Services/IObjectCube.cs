using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Fanex.Athena.LGAPI.SSAS.Services
{
    public interface IObjectCube : IDisposable
    {
        IEnumerable<IDictionary<string, object>> Query(string query);

        IEnumerable<IDictionary<string, object>> Query(string query, AdomdParameter[] parameters);

        IDataReader ExecuteReader(string query);
    }
}