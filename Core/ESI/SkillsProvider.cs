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
        private TypeInfoProvider TypeInfoProvider { get; }
        private TypeAttributeProvider TypeAttributeProvider { get; }
        private ImmutableList<Skill> Cache { get; set; }

        public SkillsProvider(RequestHandler requestHandler, CategoriesProvider categoriesProvider, TypeInfoProvider typeInfoProvider, TypeAttributeProvider typeAttributeProvider)
        {
            CategoriesProvider = categoriesProvider;
            RequestHandler = requestHandler;
            TypeInfoProvider = typeInfoProvider;
            TypeAttributeProvider = typeAttributeProvider;
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

            var typeInfos = TypeInfoProvider.Get();
            Log.Info("Creating skill tree...");
            var result = CreateSkillsFromFlastList(flatSkills.ToDictionary(a => typeInfos[a.Item1], a => a.Item2))
                            .ToImmutableList();

            Log.Info("{0} skills built.", result.Count);
            return result;
        }

        private async Task<IEnumerable<Tuple<int, ImmutableDictionary<int, int>>>> GetSkillsForGroup(int groupId)
        {
            var groupResponse = await RequestHandler.GetResponseAsync("universe", "groups", groupId);
            var objGroup = (JObject)JsonConvert.DeserializeObject(groupResponse);

            var typeIds = (JArray)objGroup.GetValue("types");

            var result = (await Task.WhenAll(typeIds.Select(async a =>
                {
                    var typeId = a.Value<int>();
                    var attrs = await TypeAttributeProvider.GetTypeAttributes(typeId);
                    if (null == attrs) return null;
                    var reqs = GetRequirements(attrs);
                    return Tuple.Create(typeId, reqs);
                })))
                .Where(a => null != a)
                .ToList();
            return result;
        }

        private IEnumerable<Skill> CreateSkillsFromFlastList(Dictionary<TypeInfo, ImmutableDictionary<int, int>> flatSkills)
        {
            var dict = new Dictionary<int, Skill>();
            int lastCount = 0;
            while (lastCount != flatSkills.Count)
            {
                lastCount = flatSkills.Count;
                var toRemove = new List<TypeInfo>();
                foreach (var flatSkill in flatSkills)
                {
                    var skip = false;
                    foreach (var dep in flatSkill.Value)
                    {
                        if (!dict.ContainsKey(dep.Key)) 
                        { 
                            skip = true; 
                            break; 
                        }
                    }

                    if (skip) continue;
                    dict[flatSkill.Key.Id] = new Skill(flatSkill.Key.Id, flatSkill.Key.Name, flatSkill.Value.Select(a => new Skill.Requirement(dict[a.Key], (int)a.Value)).ToImmutableList());
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
                decimal level;
                if (!dict.TryGetValue($"requiredSkill{n}Level", out level)) continue;

                result[(int)skillId] = (int) level;
            }           

            return result.ToImmutableDictionary();
        }

        private async Task<JObject> GetType(int typeId)
        {
            var typeResponse = await RequestHandler.GetResponseAsync("universe", "types");
            return (JObject)JsonConvert.DeserializeObject(typeResponse);
        }
    }
}