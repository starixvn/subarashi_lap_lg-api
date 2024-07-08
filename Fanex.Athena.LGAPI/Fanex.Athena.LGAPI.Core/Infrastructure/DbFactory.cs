namespace Fanex.Athena.LGAPI.Core.Infrastructure
{
    using Fanex.Data;

    public class DbFactory : IDbFactory
    {
        public IObjectDb GetObjectDb(string spName, int timeout = 120)
        {
            return ObjectDbFactory.CreateInstance(spName, timeout);
        }
    }
}