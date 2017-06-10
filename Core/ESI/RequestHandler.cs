using System;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using NLog;

namespace RedHill.Core.ESI
{
    public class RequestHandler : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly HttpClient _client = new HttpClient();
        private readonly string _requestTemplate;
        private readonly string[] _keys;
        private ESIEndpointSettings ESIEndpointSettings { get; }
        private IDistributedCache Cache { get; }

        public RequestHandler(IDistributedCache cache, IOptions<ESIEndpointSettings> endpointsettings)
        {
            Cache = cache;
            ESIEndpointSettings = endpointsettings.Value;
            _requestTemplate = $"{ESIEndpointSettings.BaseURL}/{ESIEndpointSettings.Version}/" + @"{0}" + $"/?datasource={ESIEndpointSettings.DataSource}";
            _keys = new[] { ESIEndpointSettings.DataSource, ESIEndpointSettings.Version };
        }

        public async Task<string> GetResponse(params string[] request)
        {
            var key = string.Join("/", _keys.Concat(request));
            string payload;
            if (TryGetFromCache(key, out payload))
            {
                Log.Debug("Cache hit on {0}", key);
            }
            else
            {
                Log.Debug("Cache miss on {0}", key);
                var requestUrl = string.Format(_requestTemplate, string.Join("/", request));
                Log.Info("Sending request to {0}", requestUrl);
                var response = await _client.GetAsync(requestUrl);
                var slidingExpiration = GetSlidingExpiration(response);
                Log.Info("Received response from {0}. Content length: {1}.{2}", requestUrl, response.Content.Headers.ContentLength, response.Content.Headers.Expires.HasValue ? $"  Expires: {response.Content.Headers.Expires}": string.Empty);
                payload = await response.Content.ReadAsStringAsync();
                Cache.SetString(key, payload, new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration });
                
            }           

            return payload;
        }

        private static TimeSpan? GetSlidingExpiration(HttpResponseMessage response)
        {
            if (!response.Headers.Date.HasValue) return null;
            if (!response.Content.Headers.Expires.HasValue) return null;
            return response.Content.Headers.Expires.Value - response.Headers.Date.Value;
        }

        private bool TryGetFromCache(string key, out string payload)
        {
            payload = Cache.GetString(key);
            return !string.IsNullOrWhiteSpace(payload);
        }

        void IDisposable.Dispose()
        {
            _client.Dispose();
        }
    }
}
