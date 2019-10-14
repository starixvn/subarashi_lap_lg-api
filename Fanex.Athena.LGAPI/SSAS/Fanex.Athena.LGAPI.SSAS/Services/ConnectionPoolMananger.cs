using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections.Concurrent;
using System.Data;

namespace Fanex.Athena.LGAPI.SSAS.Services
{
    public class ConnectionPool
    {
        public ConnectionPool(AdomdConnection con, string connectionString)
        {
            Connection = con;
            ConnectionString = connectionString;
            con.Disposed += (s, a) =>
            {
                this.IsDisposed = true;
                Connection = null;
            };
        }

        public bool IsDisposed { get; private set; }

        public string ConnectionString { get; private set; }

        public AdomdConnection Connection { get; private set; }

        public DateTime ValidTo { get; set; }

        public void RecordCheckIn()
        {
            IsCheckedOut = false;
            TotalCheckoutTime += DateTime.Now.Subtract(LastCheckedOut);
            LastCheckedIn = DateTime.Now;
        }

        public void RecordCheckOut()
        {
            IsCheckedOut = true;
            LastCheckedOut = DateTime.Now;
        }

        public bool IsCheckedOut { get; private set; }
        public int TimesCheckedOut { get; private set; }
        public TimeSpan TotalCheckoutTime { get; private set; }
        public DateTime LastCheckedOut { get; private set; } = DateTime.MinValue;
        public DateTime LastCheckedIn { get; private set; } = DateTime.MinValue;
    }

    public interface IConnectionPoolManager
    {
        ConnectionPool GetConnection(string connectionString);

        void ReturnConnection(ConnectionPool entry);
    }

    public class ConnectionPoolManager : IConnectionPoolManager
    {
        private static Lazy<IConnectionPoolManager> lazyInstance = new Lazy<IConnectionPoolManager>(() => new ConnectionPoolManager());

        public static IConnectionPoolManager GetInstance() => lazyInstance.Value;

        private readonly ConcurrentDictionary<string, ConcurrentStack<ConnectionPool>> _avalableConnections;

        protected ConnectionPoolManager()
        {
            _avalableConnections = new ConcurrentDictionary<string, ConcurrentStack<ConnectionPool>>();
        }

        public void ReturnConnection(ConnectionPool entry)
        {
            if (entry.Connection.State == ConnectionState.Open)
            {
                var key = entry.ConnectionString;
                entry.RecordCheckIn();
                _avalableConnections[key].Push(entry);
            }
        }

        public ConnectionPool GetConnection(string connectionString)
        {
            var key = connectionString;

            ConnectionPool connection = null;
            _avalableConnections.AddOrUpdate(key, k => new ConcurrentStack<ConnectionPool>(), (k, c) =>
            {
                while (c.TryPop(out var entry))
                {
                    if (entry.ValidTo > DateTime.Now.Subtract(TimeSpan.FromMinutes(1)))
                    {
                        entry.Connection.Dispose();
                        continue;
                    }

                    connection = entry;
                    break;
                }

                return c;
            });

            if (connection == null)
            {
                var con = new AdomdConnection(connectionString);
                connection = new ConnectionPool(con, connectionString);

                var validTo = DateTime.Now.AddHours(1); //default
                connection.ValidTo = validTo;

                con.Open();
            }

            connection.RecordCheckOut();
            return connection;
        }
    }
}