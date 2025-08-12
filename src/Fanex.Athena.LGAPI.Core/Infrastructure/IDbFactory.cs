namespace Fanex.Athena.LGAPI.Core.Infrastructure
{
    using Data;

    public interface IDbFactory
    {
        IObjectDb GetObjectDb(string spName, int timeout = 120);
    }
}