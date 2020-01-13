namespace Fanex.Athena.LGAPI.SSAS.Configuration
{
    public class CubeSetting : ICubeSetting
    {
        public string Key { get; set; }
        public string ConnectionString { get; set; }
        public int ExecuteTimeout { get; set; }
    }
}