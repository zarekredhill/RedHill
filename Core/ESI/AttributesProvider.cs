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

            await _semaphore.WaitAsync();

            try
            {
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

            // Hack -- required skill levels are currently published=false, but we need the info.
            if (name.StartsWith("requiredSkill"))  return new Attribute(id, name);

            if (!obj.GetValue("published").Value<bool>()) return null;
            return new Attribute(id, name);
        }
    }
}