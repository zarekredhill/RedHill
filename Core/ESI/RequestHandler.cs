using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NLog;

namespace RedHill.Core.ESI
{
    public class RequestHandler : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly HttpClient _client = new HttpClient();
        private readonly string _requestTemplate;
        private ESIEndpointSettings ESIEndpointSettings {get;}

        public RequestHandler(IOptions<ESIEndpointSettings> endpointsettings)
        {
            
            ESIEndpointSettings = endpointsettings.Value;
            _requestTemplate = $"{ESIEndpointSettings.BaseURL}/{ESIEndpointSettings.Version}/" + @"{0}" + $"/?datasource={ESIEndpointSettings.DataSource}";
        }

        public async Task<HttpResponseMessage> GetResponse(string request)
        {            
            var requestUrl = string.Format(_requestTemplate, request);
            Log.Info("Sending request to {0}", requestUrl);
            var response = await _client.GetAsync(requestUrl);
            Log.Info("Received response from {0}. Content length: {1}", requestUrl, response.Content.Headers.ContentLength);
            return response;
        }

        void IDisposable.Dispose()
        {
            _client.Dispose();
        }
    }
}
