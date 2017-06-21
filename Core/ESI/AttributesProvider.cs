using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedHill.Core.ESI
{
    public class AttributesProvider
    {
        private Dictionary<int, Attribute> Cache { get; } = new Dictionary<int, Attribute>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);

        private RequestHandler RequestHandler { get; }

        public AttributesProvider(RequestHandler requestHandler)
        {
            RequestHandler = requestHandler;
        }

        public async Task<Attribute> Get(int id)
        {
            Attribute result;
            if (Cache.TryGetValue(id, out result)) return result;

            var attribute = await GetAttribute(id);
            await _semaphore.WaitAsync();

            try
            {
                if (Cache.TryGetValue(id, out result)) return result;
                return Cache[id] = await GetAttribute(id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<Attribute> GetAttribute(int id)
        {
            var response = await RequestHandler.GetResponseAsync("dogma", "attributes", id);
            var obj = (JObject)JsonConvert.DeserializeObject(response);
            var name = obj.GetValue("name").Value<string>();

            return new Attribute(id, name);
        }
    }
}