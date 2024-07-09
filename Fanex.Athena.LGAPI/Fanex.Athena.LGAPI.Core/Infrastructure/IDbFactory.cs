namespace Fanex.Athena.LGAPI.Core.Infrastructure
{
    using Fanex.Data;

    public interface IDbFactory
    {
        IObjectDb GetObjectDb(string spName, int timeout = 120);
    }
}