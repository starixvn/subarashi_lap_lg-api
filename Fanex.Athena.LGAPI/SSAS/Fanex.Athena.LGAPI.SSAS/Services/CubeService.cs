using Fanex.Athena.LGAPI.SSAS.Bridge.Extensions;
using System;
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

        public IEnumerable<TResult> Query<TResult>(string query, IDictionary<string, object> parameters)
            where TResult : new()
        {
            var result = Activator.CreateInstance<List<TResult>>();
            var records = ObjectCube.Query(query, parameters);
            foreach (var record in records)
            {
                var obj = record.ToObject<TResult>();
                result.Add(obj);
            }

            return result;
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
                //
            }
            _objectCube?.Dispose();
            _objectCube = null;
        }
    }
}