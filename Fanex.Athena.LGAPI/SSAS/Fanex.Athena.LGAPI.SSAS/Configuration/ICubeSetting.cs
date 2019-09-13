namespace Fanex.Athena.LGAPI.SSAS.Configuration
{
    public interface ICubeSetting
    {
        string Key { get; set; }
        string ConnectionString { get; set; }
        int ExecuteTimeout { get; set; }
    }
}