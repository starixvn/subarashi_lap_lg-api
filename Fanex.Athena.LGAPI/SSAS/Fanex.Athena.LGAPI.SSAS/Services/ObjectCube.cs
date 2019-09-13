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
        private ICubeSetting _cubeSetting;

        private AdomdConnection _cubeConnection;

        protected AdomdConnection CubeConnection => _cubeConnection ?? (_cubeConnection = new AdomdConnection(this._cubeSetting.ConnectionString));

        private AdomdCommand _cubeCommand;

        protected AdomdCommand CubeCommand
        {
            get
            {
                if (_cubeCommand == null)
                {
                    _cubeCommand = CubeConnection.CreateCommand();
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
                throw new ArgumentException(nameof(settingKey));
            }

            _cubeSetting = CubeSettingProvider.CubeSettings.FirstOrDefault(_ => _.Key == settingKey) ?? throw new ArgumentException(nameof(_cubeSetting));
        }

        public ObjectCube(ICubeSetting cubeSetting) => _cubeSetting = cubeSetting ?? throw new ArgumentException(nameof(cubeSetting));

        public IEnumerable<IDictionary<string, object>> Query(string query)
        {
            if (CubeConnection.State == ConnectionState.Closed)
            {
                CubeConnection.Open();
            }
            CubeCommand.CommandText = query;

            IDataReader dataReader = CubeCommand.ExecuteReader();
            var records = new List<IDictionary<string, object>>();
            while (dataReader.Read())
            {
                var record = ParseRowDataToObject(dataReader);
                records.Add(record);
            }

            return records;
        }

        public IEnumerable<IDictionary<string, object>> Query(string query, AdomdParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public IDataReader ExecuteReader(string query)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _cubeSetting = null;
            }

            if (_cubeCommand != null)
            {
                _cubeCommand.Dispose();
                _cubeCommand = null;
            }

            if (_cubeConnection != null)
            {
                _cubeConnection.Close();
                _cubeConnection.Dispose();
                _cubeConnection = null;
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