using Fanex.Athena.LGAPI.SSAS.Models;
using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fanex.Athena.LGAPI.SSAS.Services
{
    public class ConnectionPoolService : IConnectionPoolService
    {
        private const int DEFAULT_EXPIRE_TIME = 10;

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
                    if (entry.Connection.State != ConnectionState.Open || IsExpire(entry))
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

        private static bool IsExpire(ConnectionPool connectionPool)
        {
            return connectionPool.LastCheckedIn.AddMinutes(DEFAULT_EXPIRE_TIME) < DateTime.Now;
        }
    }
}
