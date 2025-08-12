namespace Fanex.Athena.LGAPI.Core.Model.Configuration
{
    using System.Net.Http;

    public class PortalOptions
    {
        public PortalOptions(string address, string proxy, string accessKey)
        {
            Address = address;
            Proxy = proxy;
            AccessKey = accessKey;
        }

        public string Proxy { get; set; }

        public string Address { get; set; }

        public string AccessKey { get; set; }

        public HttpMessageHandler MessageHandler { get; set; }
    }
}