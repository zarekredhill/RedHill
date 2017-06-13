using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedHill.Core.ESI
{
    public class SkillsProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private RequestHandler RequestHandler { get; }
        private CategoriesProvider CategoriesProvider { get; }
        private TypesProvider TypesProvider { get; }
        private ImmutableList<Skill> Cache { get; set; }

        public SkillsProvider(RequestHandler requestHandler, CategoriesProvider categoriesProvider, TypesProvider typesProvider)
        {
            CategoriesProvider = categoriesProvider;
            RequestHandler = requestHandler;
            TypesProvider = typesProvider;
        }

        public async Task<ImmutableList<Skill>> Get()
        {
            if (null != Cache) return Cache;
            return Cache = await GetSkills();
        }

        private async Task<ImmutableList<Skill>> GetSkills()
        {
            Log.Info("Building skills.");

            var categories = await CategoriesProvider.Get();
            var skillsCategory = categories.Single(a => string.Equals("Skill", a.Name, StringComparison.OrdinalIgnoreCase));

            var result = (await Task.WhenAll(skillsCategory.GroupIds.Select(async groupId => await GetSkillsForGroup(groupId))))
                    .SelectMany(a => a)
                    .ToImmutableList();

            Log.Info("{0} skills built.", result.Count);
            return result;
        }

        private async Task<IEnumerable<Skill>> GetSkillsForGroup(int groupId)
        {
            var groupResponse = await RequestHandler.GetResponseAsync("universe", "groups", groupId);
            var objGroup = (JObject)JsonConvert.DeserializeObject(groupResponse);

            if (!objGroup.GetValue("published").Value<bool>()) return Enumerable.Empty<Skill>();
            var typeIds = (JArray) objGroup.GetValue("types");

            return (await Task.WhenAll(typeIds.Select(async a => 
                {
                    var type = await TypesProvider.Get(a.Value<int>());
                    if (null == type) return null;
                    return new Skill(type.Name, type.Description, type.AttributeValues.ToDictionary(b => b.Key.Name, b => b.Value).ToImmutableDictionary());
                })))
                .Where(a => null != a);
        }

        private async Task<JObject> GetType(int typeId)
        {
            var typeResponse = await RequestHandler.GetResponseAsync("universe", "types");
            var result = (JObject)JsonConvert.DeserializeObject(typeResponse);
            return result.GetValue("published").Value<bool>() ? result : null;
        }
    }
}