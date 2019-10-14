using Fanex.Athena.LGAPI.SSAS.Configuration;
using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Fanex.Athena.LGAPI.SSAS.Services
{
    public class ObjectCube : IObjectCube
    {
        private readonly IConnectionPoolManager _connectionPoolManager = ConnectionPoolManager.GetInstance();

        private readonly ICubeSetting _cubeSetting;

        private ConnectionPool _cubeConnection;

        protected ConnectionPool CubeConnection => _cubeConnection ?? (_cubeConnection = _connectionPoolManager.GetConnection(_cubeSetting.ConnectionString));

        private AdomdCommand _cubeCommand;

        protected AdomdCommand CubeCommand
        {
            get
            {
                if (_cubeCommand == null)
                {
                    _cubeCommand = CubeConnection.Connection.CreateCommand();
                    if (_cubeSetting.ExecuteTimeout > 0)
                    {
                        _cubeCommand.CommandTimeout = _cubeSetting.ExecuteTimeout;
                    }
                }

                return _cubeCommand;
            }
        }

        public ObjectCube(string settingKey)
        {
            if (string.IsNullOrWhiteSpace(settingKey))
            {
                throw new ArgumentNullException(nameof(settingKey));
            }

            _cubeSetting = CubeSettingProvider.CubeSettings.FirstOrDefault(_ => _.Key == settingKey) ?? throw new KeyNotFoundException(nameof(_cubeSetting));
        }

        public ObjectCube(ICubeSetting cubeSetting) => _cubeSetting = cubeSetting ?? throw new KeyNotFoundException(nameof(cubeSetting));

        public IEnumerable<IDictionary<string, object>> Query(string queryContent, IDictionary<string, object> parameters = null)
        {
            var records = new List<IDictionary<string, object>>();

            try
            {
                CubeCommand.CommandText = queryContent;

                if (parameters != null && parameters.Count > 0)
                {
                    SetParameters(CubeCommand, parameters);
                }

                IDataReader dataReader = CubeCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    var record = ParseRowDataToObject(dataReader);
                    records.Add(record);
                }
            }
            catch
            {
                if (_cubeCommand != null)
                {
                    _cubeCommand.Dispose();
                    _cubeCommand = null;
                }

                if (_cubeConnection?.Connection != null)
                {
                    _cubeConnection.Connection.Close(true);
                    _cubeConnection.Connection.Dispose();
                    _cubeConnection = null;
                }
            }
            finally
            {
                if(CubeConnection?.Connection != null)
                {
                    _connectionPoolManager.ReturnConnection(CubeConnection);
                }
            }

            return records;
        }

        public IDataReader ExecuteReader(string queryContent)
        {
            throw new NotImplementedException();
        }

        private static void SetParameters(AdomdCommand adomdCommand, IDictionary<string, object> parameters)
        {
            foreach (var param in parameters)
            {
                adomdCommand.Parameters.Add(new AdomdParameter(param.Key, param.Value));
            }
        }

        private static IDictionary<string, object> ParseRowDataToObject(IDataReader dataReader)
        {
            var record = new Dictionary<string, object>();
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                record.Add(dataReader.GetName(i), dataReader.GetValue(i));
            }

            return record;
        }
    }
}