using Fanex.Athena.LGAPI.SSAS.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fanex.Athena.LGAPI.SSAS.Services
{
    public class CubeService : ICubeService, IDisposable
    {
        private readonly string _settingKey;

        private IObjectCube _objectCube;

        protected IObjectCube ObjectCube
        {
            get
            {
                return _objectCube ?? (_objectCube = new ObjectCube(_settingKey));
            }
        }

        public CubeService(string settingKey)
        {
            _settingKey = settingKey;
        }

        public IEnumerable<TResult> Query<TResult>(string query)
            where TResult : IExtractQueryResult<TResult>, new()
        {
            var result = Activator.CreateInstance<IList<TResult>>();
            var records = ObjectCube.Query(query);
            foreach (var record in records)
            {
                var obj = Activator.CreateInstance<TResult>();
                result.Add(obj.ToObject(record));
            }

            return result;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}