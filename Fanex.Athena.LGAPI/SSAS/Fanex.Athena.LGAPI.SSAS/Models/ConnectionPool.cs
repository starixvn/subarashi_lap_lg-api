using Microsoft.AnalysisServices.AdomdClient;
using System;

namespace Fanex.Athena.LGAPI.SSAS.Models
{
    public class ConnectionPool
    {
        public ConnectionPool(AdomdConnection con, string connectionString)
        {
            Connection = con;
            ConnectionString = connectionString;
            con.Disposed += (s, a) =>
            {
                IsDisposed = true;
                con = null;
            };
        }

        public bool IsDisposed { get; private set; }

        public string ConnectionString { get; private set; }

        public AdomdConnection Connection { get; private set; }

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
}