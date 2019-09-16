using Fanex.Athena.LGAPI.SSAS.Bridge.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fanex.Athena.LGAPI.SSAS.Bridge.Services
{
    public class SsasBridgeService : ISsasBridgeService
    {
        private readonly string _connectionKey;
        private readonly string _baseUrl;

        public SsasBridgeService(string connectionKey, string baseUrl)
        {
            _connectionKey = connectionKey;
            _baseUrl = baseUrl;
        }

        public async Task<IEnumerable<TResult>> QueryAsync<TResult>(string query, IDictionary<string, object> parameters = null, string bridgeUrl = "ssas/query")
            where TResult : new()
        {
            var result = Activator.CreateInstance<List<TResult>>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUrl);
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, bridgeUrl);
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(new { query, connectionKey = _connectionKey, parameters }), Encoding.UTF8, "application/json");
                var response = await client.SendAsync(requestMessage);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var records = JsonConvert.DeserializeObject<IEnumerable<IDictionary<string, object>>>(content);
                    foreach (var record in records)
                    {
                        var obj = record.ToObject<TResult>();
                        result.Add(obj);
                    }
                }
                else
                {
                    throw new Exception(content);
                }
            }

            return result;
        }
    }
}