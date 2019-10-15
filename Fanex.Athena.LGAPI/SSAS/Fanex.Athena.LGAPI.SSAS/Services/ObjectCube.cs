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
        private readonly Lazy<IConnectionPoolService> _connectionPoolManager = new Lazy<IConnectionPoolService>(() => ConnectionPoolService.GetInstance());

        private readonly ICubeSetting _cubeSetting;

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

            var connectionPool = _connectionPoolManager.Value.GetConnection(_cubeSetting.ConnectionString);
            try
            {
                using (var command = connectionPool.Connection.CreateCommand())
                {
                    command.CommandTimeout = _cubeSetting.ExecuteTimeout;
                    command.CommandText = queryContent;

                    if (parameters != null && parameters.Count > 0)
                    {
                        SetParameters(command, parameters);
                    }

                    using (IDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var record = ParseRowDataToObject(dataReader);
                            records.Add(record);
                        }
                    }
                }

                _connectionPoolManager.Value.ReturnConnection(connectionPool);
            }
            catch (Exception ex)
            {
                connectionPool.Connection.Close(true);
                connectionPool.Connection.Dispose();
                throw ex;
            }

            return records;
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