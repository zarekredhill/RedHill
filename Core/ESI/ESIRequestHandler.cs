using System;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;
// using NLog.Extensions.Logging;

namespace RedHill.Core.ESI
{
    public class ESIRequestHandler : IDisposable
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly HttpClient _client = new HttpClient();
        private readonly string _requestTemplate;

        public ESIRequestHandler(string baseUrl, string version, string datasource)
        {
            _requestTemplate = $"{baseUrl}/{version}/" + @"{0}" + $"/?datasource={datasource}";
        }

        public async Task<HttpResponseMessage> GetResponse(string request)
        {            
            var requestUrl = string.Format(_requestTemplate, request);
            _log.Info("Sending request to {0}", requestUrl);
            var response = await _client.GetAsync(requestUrl);
            _log.Info("Received response from {0}. Content length: {1}", requestUrl, response.Content.Headers.ContentLength);
            return response;
        }

        void IDisposable.Dispose()
        {
            _client.Dispose();
        }
    }
}
