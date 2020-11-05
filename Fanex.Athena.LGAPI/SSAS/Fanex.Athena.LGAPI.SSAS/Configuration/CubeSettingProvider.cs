using System.Collections.Generic;
using System.Configuration;

namespace Fanex.Athena.LGAPI.SSAS.Configuration
{
    public static class CubeSettingProvider
    {
        private const int ExecuteTimeout = 120;

        public static IList<ICubeSetting> CubeSettings { private set; get; }

        static CubeSettingProvider()
        {
            CubeSettings = new List<ICubeSetting>();
        }

        public static void UseTabularCube()
        {
            var connectionsStrs = ConfigurationManager.ConnectionStrings;
            foreach (ConnectionStringSettings conn in connectionsStrs)
            {
                if (conn.Name.StartsWith("SSAS_"))
                {
                    CubeSettings.Add(new CubeSetting
                    {
                        Key = conn.Name,
                        ConnectionString = conn.ConnectionString,
                        ExecuteTimeout = ExecuteTimeout
                    });
                }
            }
        }
    }
}
