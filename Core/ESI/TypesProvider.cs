using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedHill.Core.ESI
{
    public class TypesProvider
    {
        private Dictionary<int, Type> Cache { get; } = new Dictionary<int, Type>();
        private RequestHandler RequestHandler { get; }
        private AttributesProvider AttributesProvider { get; }
        private StaticTypeDataProvider StaticTypeDataProvider { get; }

        public TypesProvider(RequestHandler requestHandler, AttributesProvider attributesProvider, StaticTypeDataProvider staticTypeDataProvider)
        {
            RequestHandler = requestHandler;
            AttributesProvider = attributesProvider;
            StaticTypeDataProvider = staticTypeDataProvider;
        }

        public async Task<Type> Get(int id)
        {
            Type result;
            if (Cache.TryGetValue(id, out result)) return result;

            var attributes = await GetTypeAttributes(id);
            var staticData = StaticTypeDataProvider.StaticTypeData[id];
            result = Cache[id] = new Type(id, staticData.Name, staticData.Description, attributes);
            return result;
        }

        private async Task<ImmutableDictionary<Attribute, decimal>> GetTypeAttributes(int id)
        {
            var response = await RequestHandler.GetResponseAsync("universe", "types", id);
            var obj = (JObject)JsonConvert.DeserializeObject(response);
            return await GetAttributes(obj);
        }

        private async Task<ImmutableDictionary<Attribute, decimal>> GetAttributes(JObject responseObj)
        {
            var result = new Dictionary<Attribute, decimal>();
            var attrs = (JArray)responseObj.GetValue("dogma_attributes");
            foreach (var attr in attrs?.AsJEnumerable()?.Cast<JObject>())
            {
                var id = attr.GetValue("attribute_id").Value<int>();
                var attribute = await AttributesProvider.Get(id);
                if (null == attribute) continue;
                result[attribute] = attr.GetValue("value").Value<decimal>();
            }
            return result.ToImmutableDictionary();
        }
    }
}