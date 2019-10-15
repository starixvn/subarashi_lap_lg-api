using Fanex.Athena.LGAPI.SSAS.Models;

namespace Fanex.Athena.LGAPI.SSAS.Services
{
    public interface IConnectionPoolService
    {
        ConnectionPool GetConnection(string connectionString);

        void ReturnConnection(ConnectionPool entry);
    }
}