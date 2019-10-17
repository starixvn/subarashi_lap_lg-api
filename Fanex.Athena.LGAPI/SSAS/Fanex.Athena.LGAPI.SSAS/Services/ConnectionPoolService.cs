using Fanex.Athena.LGAPI.SSAS.Models;
using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data;

namespace Fanex.Athena.LGAPI.SSAS.Services
{
    public class ConnectionPoolService : IConnectionPoolService
    {
        private int? _expireTime;
        public int ExpireTime
        {
            get
            {
                if(!_expireTime.HasValue)
                {  
                    _expireTime = Convert.ToInt32(ConfigurationManager.AppSettings["SSAS-Connection-Pool-Expire-Time"]);
                }
                return _expireTime.Value;
            }   
        }

        private static Lazy<IConnectionPoolService> lazyInstance = new Lazy<IConnectionPoolService>(() => new ConnectionPoolService());

        public static IConnectionPoolService GetInstance() => lazyInstance.Value;

        private readonly ConcurrentDictionary<string, ConcurrentStack<ConnectionPool>> _avalableConnections;

        protected ConnectionPoolService()
        {
            _avalableConnections = new ConcurrentDictionary<string, ConcurrentStack<ConnectionPool>>();
        }

        public void ReturnConnection(ConnectionPool entry)
        {
            if (entry.Connection.State != ConnectionState.Open)
            {
                entry.Connection.Dispose();
                return;
            }
            var key = entry.ConnectionString;
            entry.RecordCheckIn();
            _avalableConnections[key].Push(entry);
        }

        public ConnectionPool GetConnection(string connectionString)
        {
            ConnectionPool connectionPool = null;
            _avalableConnections.AddOrUpdate(connectionString, key => new ConcurrentStack<ConnectionPool>(), (key, stack) =>
            {
                while (connectionPool == null && stack.TryPop(out ConnectionPool entry))
                {
                    if (entry.Connection.State != ConnectionState.Open || IsExpire(entry, ExpireTime))
                    {
                        entry.Connection.Dispose();
                        continue;
                    }
                    connectionPool = entry;
                }

                return stack;
            });

            if (connectionPool == null)
            {
                var adomdConn = new AdomdConnection(connectionString);
                connectionPool = new ConnectionPool(adomdConn, connectionString);
                adomdConn.Open();
            }

            connectionPool.RecordCheckOut();

            return connectionPool;
        }

        private static bool IsExpire(ConnectionPool connectionPool, int expireTime)
        {
            return connectionPool.LastCheckedIn.AddMinutes(expireTime) < DateTime.Now;
        }
    }
}