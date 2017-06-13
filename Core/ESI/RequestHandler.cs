using System;
using System.Threading;
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
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);

        public RequestHandler(IDistributedCache cache, IOptions<ESIEndpointSettings> endpointsettings)
        {
            Cache = cache;
            ESIEndpointSettings = endpointsettings.Value;
            _requestTemplate = $"{ESIEndpointSettings.BaseURL}/{ESIEndpointSettings.Version}/" + @"{0}" + $"/?datasource={ESIEndpointSettings.DataSource}&language=en-us";
            _keys = new[] { ESIEndpointSettings.DataSource, ESIEndpointSettings.Version };
        }

        public async Task<string> GetResponseAsync(params object[] request)
        {
            var key = string.Join("/", _keys.Concat(request));
            var payload = Cache.GetString(key);
            if (payload != null)
            {
                Log.Debug("Cache hit on {0}", key);
                return payload;
            }
            
            Log.Debug("Cache miss on {0}", key);
            var requestUrl = string.Format(_requestTemplate, string.Join("/", request));
            Log.Info("Sending request to {0}", requestUrl);
            var response = await _client.GetAsync(requestUrl);
            var slidingExpiration = GetSlidingExpiration(response);
            await _semaphore.WaitAsync();

            try
            {
                payload = Cache.GetString(key);
                if (payload != null) return payload;
                Log.Info("Received response from {0}. Content length: {1}.{2}", requestUrl, response.Content.Headers.ContentLength, response.Content.Headers.Expires.HasValue ? $"  Expires: {response.Content.Headers.Expires}": string.Empty);
                payload = await response.Content.ReadAsStringAsync();
                Cache.SetString(key, payload, new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration });
                return payload;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static TimeSpan? GetSlidingExpiration(HttpResponseMessage response)
        {
            if (!response.Headers.Date.HasValue) return null;
            if (!response.Content.Headers.Expires.HasValue) return null;
            var result = response.Content.Headers.Expires.Value - response.Headers.Date.Value;
            if (result > TimeSpan.Zero) return result;
            return null;
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
