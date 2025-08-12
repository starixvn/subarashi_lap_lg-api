namespace Fanex.Athena.LGAPI.Core.Infrastructure
{
    using Data;

    public class DbFactory : IDbFactory
    {
        public IObjectDb GetObjectDb(string spName, int timeout = 120)
        {
            return ObjectDbFactory.CreateInstance(spName, timeout);
        }
    }
}