using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RedHill.Core
{
    public class ESIRequestHandler : IDisposable
    {
        private readonly ILogger _log;
        private readonly HttpClient _client = new HttpClient();
        private readonly string _requestTemplate;

        public ESIRequestHandler(string baseUrl, string version, string datasource, ILogger log)
        {
            _log = log;
            _requestTemplate = $"{baseUrl}/{version}/" + @"{0}" + $"/?datasource={datasource}";
        }

        public async Task<string> GetResponse(string request)
        {            
            var requestUrl = string.Format(_requestTemplate, request);
            _log.LogInformation("Sending request to {0}", requestUrl);
            var response = await _client.GetStringAsync(requestUrl);
            return response;
        }

        void IDisposable.Dispose()
        {
            _client.Dispose();
        }
    }
}
