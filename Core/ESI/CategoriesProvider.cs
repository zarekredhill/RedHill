using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace RedHill.Core.ESI
{
    public class CategoriesProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private ImmutableList<Category> Cache { get; set; }

        private RequestHandler RequestHandler { get; }

        public CategoriesProvider(RequestHandler requestHandler)
        {
            RequestHandler = requestHandler;
        }

        public async Task<ImmutableList<Category>> Get()
        {
            if (null != Cache) return Cache;
            return Cache = await GetCategories();
        }

        private async Task<ImmutableList<Category>> GetCategories()
        {
            var response = await RequestHandler.GetResponseAsync("universe", "categories");

            var categoryIds = ((JArray)JsonConvert.DeserializeObject(response)).Select(a => a.Value<int>());

            var categories = (await Task.WhenAll(categoryIds.Select(async a => await GetCategoryAsync(a))))
                             .Where(a => a != null);

            Log.Trace("Categories: {0}", string.Join(", ", categories.Select(a => $"[Id: {a.Id}, Name: {a.Name}]")));

            return categories.ToImmutableList();
        }

        private async Task<Category> GetCategoryAsync(int categoryId)
        {
            var categoryResponse = await RequestHandler.GetResponseAsync("universe", "categories", categoryId);
            var obj = (JObject)JsonConvert.DeserializeObject(categoryResponse);

            var name = obj.GetValue("name").Value<string>();
            var groupIds = ((JArray)obj.GetValue("groups")).Select(a => a.Value<int>()).ToImmutableList();

            return new Category(categoryId, name, groupIds);
        }
    }
}