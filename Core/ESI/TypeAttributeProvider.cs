using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedHill.Core.ESI
{
    public class TypeAttributeProvider
    {
        private RequestHandler RequestHandler { get; }
        private AttributesProvider AttributesProvider { get; }

        public TypeAttributeProvider(RequestHandler requestHandler, AttributesProvider attributesProvider)
        {
            RequestHandler = requestHandler;
            AttributesProvider = attributesProvider;
        }

        public async Task<ImmutableDictionary<Attribute, decimal>> GetTypeAttributes(int id)
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