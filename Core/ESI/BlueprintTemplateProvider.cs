using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using NLog;
using RedHill.Core.Util;

namespace RedHill.Core.ESI
{
    public class BlueprintTemplateProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private TypeInfoProvider TypeInfoProvider { get; }
        private SkillsProvider SkillsProvider { get; }
        private StaticFileProvider StaticFileProvider { get; }
        private ImmutableDictionary<TypeInfo, BlueprintTemplate> Cache { get; set; }
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private ImmutableDictionary<int, TypeInfo> TypeInfos => TypeInfos ?? TypeInfoProvider.Get();

        public BlueprintTemplateProvider(TypeInfoProvider typeInfoProvider, SkillsProvider skillsProvider, StaticFileProvider staticFileProvider)
        {
            TypeInfoProvider = typeInfoProvider;
            SkillsProvider = skillsProvider;
            StaticFileProvider = staticFileProvider;
        }

        public async Task<ImmutableDictionary<TypeInfo, BlueprintTemplate>> Get()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (null != Cache) return Cache;
            }
            finally
            {
                _semaphore.Release();
            }

            var data = await GetData();
            await _semaphore.WaitAsync();
            try
            {
                if (null != Cache) return Cache;
                return Cache = await GetData();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<ImmutableDictionary<TypeInfo, BlueprintTemplate>> GetData()
        {
            var typeInfos = TypeInfoProvider.Get();
            var data = await Parse(StaticFileProvider.Get("sde", "fsd", "blueprints.yaml"));
            Log.Info("Blueprint templates loaded ({0}).", data.Count);
            return data;
        }
        private async Task<ImmutableDictionary<TypeInfo, BlueprintTemplate>> Parse(YamlDocument yamlDocument)
        {
            var skillsLookup = (await SkillsProvider.Get()).ToImmutableDictionary(a => a.Type.Id);
            var dict = new Dictionary<TypeInfo, BlueprintTemplate>();
            foreach (var kvp in ((YamlMappingNode)yamlDocument.RootNode).Children)
            {
                var keyNode = (YamlScalarNode)kvp.Key;
                var id = int.Parse(keyNode.Value);
                var type = TypeInfos[id];

                var blueprint = Parse(skillsLookup, type, keyNode, (YamlMappingNode)kvp.Value);
                if (null == blueprint) continue;

                dict[blueprint.Type] = blueprint;
            }
            return dict.ToImmutableDictionary();
        }

        private BlueprintTemplate Parse(ImmutableDictionary<int, Skill> skillsLookup, TypeInfo type, YamlScalarNode keyNode, YamlMappingNode valueNode)
        {
            int copyTimeSecond;

            var times = new[] { "copying", "research_material", "research_time" }
                .Select(a => valueNode.TryGetScalar<int>(out copyTimeSecond, "activities", a, "time")
               ? (TimeSpan?)TimeSpan.FromSeconds(copyTimeSecond)
               : null).ToArray();

            var copyTime = times[0];
            var materialResearchTime = times[1];
            var timeResearchTime = times[2];
            var maxProductionLimit = valueNode.GetScalar<int>("maxProductionLimit");

            YamlMappingNode manufacturingNode;
            if (!valueNode.TryGet<YamlMappingNode>(out manufacturingNode, "activities", "manufacturing")) return null;

            var manufacturing = GetManufacturingDetails(skillsLookup, manufacturingNode);

            return new BlueprintTemplate(type, maxProductionLimit, copyTime, materialResearchTime, timeResearchTime, manufacturing);
        }

        private BlueprintTemplate.ManufacturingDetails GetManufacturingDetails(ImmutableDictionary<int, Skill> skillsLookup, YamlMappingNode node)
        {
            var time = TimeSpan.FromSeconds(node.GetScalar<int>("time"));
            YamlSequenceNode productsNode;
            if (!node.TryGet<YamlSequenceNode>(out productsNode, "products")) return null;
            var productNode = productsNode.Single();

            var product = TypeInfos[productNode.GetScalar<int>("typeID")];
            if (null == product) return null;

            var quantity = productNode.GetScalar<int>("quantity");

            YamlSequenceNode skillsNode;
            var skillReqs = (node.TryGet<YamlSequenceNode>(out skillsNode, "skills")
                    ? skillsNode.Select(a => new Skill.Requirement(skillsLookup[a.GetScalar<int>("typeID")], a.GetScalar<int>("level")))
                    : Enumerable.Empty<Skill.Requirement>())
                .ToImmutableList();

            return new BlueprintTemplate.ManufacturingDetails(time, product, quantity, skillReqs);
        }
    }
}
