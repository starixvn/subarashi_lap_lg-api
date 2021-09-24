using Fanex.Athena.LGAPI.Core.Model.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Fanex.Athena.LGAPI.Core.Service
{
    public class PortalService : IPortalService, IDisposable
    {
        private readonly HttpClient _client;
        private readonly PortalOptions _options;

        public PortalService(PortalOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            var proxy = !string.IsNullOrEmpty(options.Proxy) ? new WebProxy
            {
                Address = new Uri(options.Proxy),
                UseDefaultCredentials = true,
                BypassProxyOnLocal = false
            } : null;

            _client = new HttpClient(_options.MessageHandler ?? new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false, Proxy = proxy })
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        public Task<HttpResponseMessage> GetSpecialLeagues(string siteName)
        {
            const string path = "/ExternalApi/GetSpecialLeagueList";
            return HandleRequest(path, new Dictionary<string, string> { { nameof(siteName), siteName } });
        }

        public Task<HttpResponseMessage> GetLimitLeagues(string siteName, int sportType, int leagueGroupId, string langCode)
        {
            const string path = "/ExternalApi/GetLimitLeagues";
            return HandleRequest(path, new Dictionary<string, string> {
                { nameof(siteName), siteName },
                { nameof(sportType), sportType.ToString() },
                { nameof(leagueGroupId), leagueGroupId.ToString() },
                { nameof(langCode), langCode },
            });
        }

        public Task<HttpResponseMessage> GetLeagueGroups(int sportType, string langCode)
        {
            const string path = "/ExternalApi/GetLimitLeagueGroups";
            return HandleRequest(path, new Dictionary<string, string> {
                { nameof(sportType), sportType.ToString() },
                { nameof(langCode), langCode }
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3445:Exceptions should not be explicitly rethrown", Justification = "Howard")]
        private async Task<HttpResponseMessage> HandleRequest(string path, IDictionary<string, string> queryData)
        {
            using (var request = CreateHttpRequest(path, queryData))
            {
                var response = null as HttpResponseMessage;
                try
                {
                    response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    return response;
                }
                catch (Exception ex)
                {
                    response?.Dispose();
                    throw ex;
                }
            }
        }

        public HttpRequestMessage CreateHttpRequest(string path, IDictionary<string, string> queryData)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (var item in queryData)
            {
                query[item.Key] = item.Value;
            }
            query[nameof(_options.AccessKey)] = _options.AccessKey;

            var uri = new Uri($"{_options.Address.Trim('/')}/{path.Trim('/')}?{query}");

            return new HttpRequestMessage(HttpMethod.Get, uri);
        }

        ~PortalService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _client?.Dispose();
        }
    }
}