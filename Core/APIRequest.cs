using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedHill.Core
{
    public class ESIRequest
    {
        private static readonly string requestUrl = @"https://esi.tech.ccp.is/latest/universe/categories/?datasource=tranquility";

        public async Task<string> GetResponse()
        {
            using(var http = new HttpClient())
            {
                return await http.GetStringAsync(requestUrl);
            }
        }
    }
}
