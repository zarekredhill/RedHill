using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace RedHill.Core.ESI
{
    public class CategoriesProvider
    {
        private RequestHandler RequestHandler { get; }

        public CategoriesProvider(RequestHandler requestHandler)
        {
            RequestHandler = requestHandler;
        }

        public async Task<ImmutableList<Category>> Get()
        {
            var result = new List<Category>();
            var response = await RequestHandler.GetResponse("universe", "categories");

            var categoryIds = ((JArray)JsonConvert.DeserializeObject(response)).Select(a => a.Value<int>());
            
            foreach(var categoryId in categoryIds)
            {
                var categoryResponse = await RequestHandler.GetResponse("universe", "categories", categoryId.ToString());
                var obj = (JObject) JsonConvert.DeserializeObject(categoryResponse);
                if (!obj.GetValue("published").Value<bool>()) continue;
                
                var name = obj.GetValue("name").Value<string>();
                var groupIds = ((JArray)obj.GetValue("groups")).Select(a => a.Value<int>()).ToImmutableList();

                result.Add(new Category(categoryId, name, groupIds));
            }
            return result.ToImmutableList();
        }
    }
}