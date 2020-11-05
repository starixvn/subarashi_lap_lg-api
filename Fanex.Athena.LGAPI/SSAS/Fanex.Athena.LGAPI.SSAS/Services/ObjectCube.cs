using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Fanex.Athena.LGAPI.SSAS.Configuration;

namespace Fanex.Athena.LGAPI.SSAS.Services
{
    public class ObjectCube : IObjectCube
    {
        private readonly Lazy<IConnectionPoolService> _connectionPoolService = new Lazy<IConnectionPoolService>(() => ConnectionPoolService.GetInstance());

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

        public IEnumerable<IDictionary<string, object>> Query(string contentQuery)
        {
            var records = new List<IDictionary<string, object>>();

            var connectionPool = _connectionPoolService.Value.GetConnection(_cubeSetting.ConnectionString);
            try
            {
                using (var command = connectionPool.Connection.CreateCommand())
                {
                    command.CommandTimeout = _cubeSetting.ExecuteTimeout;
                    command.CommandText = contentQuery;

                    using (IDataReader dataReader = command.ExecuteReader())
                    {
                        var fields = GetFields(dataReader);

                        while (dataReader.Read())
                        {
                            var record = ParseRowDataToObject(dataReader, fields);
                            records.Add(record);
                        }
                    }
                }

                _connectionPoolService.Value.ReturnConnection(connectionPool);
            }
            catch (Exception)
            {
                connectionPool.Connection.Close(true);
                connectionPool.Connection.Dispose();
                throw;
            }

            return records;
        }

        public IEnumerable<IEnumerable<IDictionary<string, object>>> QueryMultiple(IEnumerable<string> contentQueries)
        {
            foreach (var contentQuery in contentQueries)
            {
                var result = Query(contentQuery);
                yield return result;
            }
        }

        private static IDictionary<string, object> ParseRowDataToObject(IDataReader dataReader, IEnumerable<string> fields)
        {
            var record = new Dictionary<string, object>();
            foreach (var field in fields)
            {
                record.Add(field, dataReader[field]);
            }

            return record;
        }

        private static IEnumerable<string> GetFields(IDataReader dataReader)
        {
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                yield return dataReader.GetName(i);
            }
        }
    }
}
