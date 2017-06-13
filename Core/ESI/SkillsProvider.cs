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

            var flatSkills = (await Task.WhenAll(skillsCategory.GroupIds.Select(async groupId => await GetSkillsForGroup(groupId))))
                    .SelectMany(a => a);

            var result = CreateSkillsFromFlastList(flatSkills.ToDictionary(a => a.Item1, a => Tuple.Create(a.Item2, a.Item3, a.Item4)))
                            .ToImmutableList();

            Log.Info("{0} skills built.", result.Count);
            return result;
        }

        private async Task<IEnumerable<Tuple<int, string, string, ImmutableDictionary<int, int>>>> GetSkillsForGroup(int groupId)
        {
            var groupResponse = await RequestHandler.GetResponseAsync("universe", "groups", groupId);
            var objGroup = (JObject)JsonConvert.DeserializeObject(groupResponse);

            if (!objGroup.GetValue("published").Value<bool>()) return Enumerable.Empty<Tuple<int, string, string, ImmutableDictionary<int, int>>>();
            var typeIds = (JArray)objGroup.GetValue("types");

            var result = (await Task.WhenAll(typeIds.Select(async a =>
                {
                    var type = await TypesProvider.Get(a.Value<int>());
                    if (null == type) return null;
                    var reqs = GetRequirements(type.AttributeValues);
                    return Tuple.Create(type.Id, type.Name, type.Description, reqs);
                })))
                .Where(a => null != a)
                .ToList();
            return result;
        }

        private IEnumerable<Skill> CreateSkillsFromFlastList(Dictionary<int, Tuple<string, string, ImmutableDictionary<int, int>>> flatSkills)
        {
            var dict = new Dictionary<int, Skill>();
            while (flatSkills.Any())
            {
                var toRemove = new List<int>();
                foreach (var flatSkill in flatSkills)
                {
                    var skip = false;
                    foreach (var dep in flatSkill.Value.Item3)
                    {
                        if (!dict.ContainsKey(dep.Key)) 
                        { 
                            skip = true; 
                            break; 
                        }
                    }

                    if (skip) continue;
                    dict[flatSkill.Key] = new Skill(flatSkill.Key, flatSkill.Value.Item1, flatSkill.Value.Item2, flatSkill.Value.Item3.ToImmutableDictionary(a => dict[a.Key], a => (int)a.Value));
                    toRemove.Add(flatSkill.Key);
                }


                foreach (var a in toRemove) flatSkills.Remove(a);
            }
            return dict.Values;
        }

        private static ImmutableDictionary<int, int> GetRequirements(ImmutableDictionary<Attribute, decimal> attrs)
        {
            var result = new Dictionary<int, int>();
            var dict = attrs.ToDictionary(a => a.Key.Name, a => a.Value);
            for (var n = 1; ; n++)
            {
                decimal skillId;
                if (!dict.TryGetValue($"requiredSkill{n}", out skillId)) break;
                var level = (int)dict[$"requiredSkill{n}Level"];

                result[(int)skillId] = level;
            }
            return result.ToImmutableDictionary();
        }

        private async Task<JObject> GetType(int typeId)
        {
            var typeResponse = await RequestHandler.GetResponseAsync("universe", "types");
            var result = (JObject)JsonConvert.DeserializeObject(typeResponse);
            return result.GetValue("published").Value<bool>() ? result : null;
        }
    }
}