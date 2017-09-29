using System;
using System.Linq;
using System.Collections.Immutable;
using System.Threading.Tasks;
using NLog;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedHill.Core.ESI
{
    public class PlanetaryCommoditiesProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private CategoriesProvider CategoriesProvider { get; }
        private TypeInfoProvider TypeInfoProvider { get; }
        private RequestHandler RequestHandler { get; }

        private ImmutableList<PlanetaryCommodity> Cache { get; set; }

        public PlanetaryCommoditiesProvider(RequestHandler requestHandler, TypeInfoProvider typeInfoProvider, CategoriesProvider categoriesProvider)
        {
            TypeInfoProvider = typeInfoProvider;
            CategoriesProvider = categoriesProvider;
            RequestHandler = requestHandler;
        }

        public async Task<ImmutableList<PlanetaryCommodity>> Get()
        {
            if (null != Cache) return Cache;
            return Cache = await GetInternal();
        }

        private async Task<ImmutableList<PlanetaryCommodity>> GetInternal()
        {
            Log.Info("Building planetary commodities.");

            var categories = await CategoriesProvider.Get();
            var skillsCategory = categories.Single(a => string.Equals("Planetary Commodities", a.Name, StringComparison.OrdinalIgnoreCase));

            var typeIds = (await Task.WhenAll(skillsCategory.GroupIds.Select(async groupId => await GetForGroup(groupId))))
                    .SelectMany(a => a);

            var result = TypeInfoProvider.Get()
                .Where(a => typeIds.Contains(a.Key))
                .Select(a => new PlanetaryCommodity(a.Key, a.Value.Name, a.Value.Volume.Value))
                .ToImmutableList();

            Log.Info("{0} planetary commodities built.", result.Count);
            return result;
        }

        private async Task<IEnumerable<int>> GetForGroup(int groupId)
        {
            var groupResponse = await RequestHandler.GetResponseAsync("universe", "groups", groupId);
            var objGroup = (JObject)JsonConvert.DeserializeObject(groupResponse);

            var typeIds = (JArray)objGroup.GetValue("types");
            return typeIds.Select(a => a.Value<int>());
        }

    }
}