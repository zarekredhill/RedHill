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
        private ImmutableDictionary<int, TypeInfo> TypeInfos { get; set; }
        private ImmutableDictionary<int, Skill> Skills { get; set; }

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

            await _semaphore.WaitAsync();
            Skills = Skills ?? (await SkillsProvider.Get()).ToImmutableDictionary(a => a.Type.Id);
            TypeInfos = TypeInfos ?? TypeInfoProvider.Get();
            var data = await GetData();
            try
            {
                if (null != Cache) return Cache;
                return Cache = data;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<ImmutableDictionary<TypeInfo, BlueprintTemplate>> GetData()
        {
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
                TypeInfo type;
                if (!TypeInfos.TryGetValue(id, out type)) continue;
                Log.Trace("Parsing blueprint {0}", id);

                var blueprint = Parse(type, keyNode, (YamlMappingNode)kvp.Value);
                if (null == blueprint) continue;

                dict[blueprint.Type] = blueprint;
            }
            return dict.ToImmutableDictionary();
        }

        private BlueprintTemplate Parse(TypeInfo type, YamlScalarNode keyNode, YamlMappingNode valueNode)
        {
            var activities = valueNode.Get<YamlMappingNode>("activities");
            var copyingDetails = GetCopyingDetails(activities, valueNode.GetScalar<int>("maxProductionLimit"));
            var manufacturingDetails = GetManufacturingDetails(activities);

            if (manufacturingDetails == null) return null;

            var materialResearch = GetResearchDetails(activities, "research_material");
            var timeResearch = GetResearchDetails(activities, "research_time");
            
            return new BlueprintTemplate(type, copyingDetails, manufacturingDetails, materialResearch, timeResearch);
        }

        private BlueprintTemplate.CopyingDetails GetCopyingDetails(YamlMappingNode activities, int maxRunsPerCopy)
        {
            YamlMappingNode mainNode;
            if (!activities.TryGet<YamlMappingNode>(out mainNode, "copying")) return null;

            YamlSequenceNode materialsNode;
            var materials = (mainNode.TryGet<YamlSequenceNode>(out materialsNode, "materials")
                    ? materialsNode.Children.Cast<YamlMappingNode>()
                        .Select(a => 
                            {
                                TypeInfo type = null;
                                TypeInfos.TryGetValue(a.GetScalar<int>("typeID"), out type);
                                return Tuple.Create(type, a.GetScalar<int>("quantity"));
                            }
                        )
                        .Where(a => null != a.Item1)
                        .ToDictionary(a => a.Item1, a => a.Item2)
                    : new Dictionary<TypeInfo, int>())
                .ToImmutableDictionary();

            YamlSequenceNode skillsNode;
            var requirements = (mainNode.TryGet<YamlSequenceNode>(out skillsNode, "skills")
                    ? skillsNode.Children.Cast<YamlMappingNode>().Select(a => new Skill.Requirement(Skills[a.GetScalar<int>("typeID")], a.GetScalar<int>("level")))
                    : Enumerable.Empty<Skill.Requirement>())
                .ToImmutableList();

            var time = TimeSpan.FromSeconds(mainNode.GetScalar<int>("time"));

            return new BlueprintTemplate.CopyingDetails(time, maxRunsPerCopy, requirements, materials);
        }

        private BlueprintTemplate.ManufacturingDetails GetManufacturingDetails(YamlMappingNode node)
        {
            YamlMappingNode mainNode;
            if (!node.TryGet<YamlMappingNode>(out mainNode, "manufacturing")) return null;

            YamlSequenceNode materialsNode;
            var materials = (mainNode.TryGet<YamlSequenceNode>(out materialsNode, "materials")
                    ? materialsNode.Children.Cast<YamlMappingNode>()
                        .Select(a => 
                            {
                                TypeInfo type = null;
                                TypeInfos.TryGetValue(a.GetScalar<int>("typeID"), out type);
                                return Tuple.Create(type, a.GetScalar<int>("quantity"));
                            }
                        )
                        .Where(a => null != a.Item1)
                        .ToDictionary(a => a.Item1, a => a.Item2)
                    : new Dictionary<TypeInfo, int>())
                .ToImmutableDictionary();

            YamlSequenceNode skillsNode;
            var requirements = (mainNode.TryGet<YamlSequenceNode>(out skillsNode, "skills")
                    ? skillsNode.Children.Cast<YamlMappingNode>().Select(a => new Skill.Requirement(Skills[a.GetScalar<int>("typeID")], a.GetScalar<int>("level")))
                    : Enumerable.Empty<Skill.Requirement>())
                .ToImmutableList();

            YamlSequenceNode productsNode;
            var product = (mainNode.TryGet<YamlSequenceNode>(out productsNode, "products")
                    ? productsNode.Children.Cast<YamlMappingNode>()
                        .Select(a => 
                                {
                                    TypeInfo type = null;
                                    TypeInfos.TryGetValue(a.GetScalar<int>("typeID"), out type);
                                    return Tuple.Create(type, a.GetScalar<int>("quantity"));
                                })
                        .Where(a => null != a.Item1)
                    : Enumerable.Empty<Tuple<TypeInfo, int>>())
                .SingleOrDefault();
            if (null == product) return null;

            var time = TimeSpan.FromSeconds(mainNode.GetScalar<int>("time"));

            return new BlueprintTemplate.ManufacturingDetails(time, product.Item1, product.Item2, requirements, materials);
        }

        private BlueprintTemplate.ResearchDetails GetResearchDetails(YamlMappingNode activities, string activity)
        {
            YamlMappingNode mainNode;
            if (!activities.TryGet<YamlMappingNode>(out mainNode, activity)) return null;

            YamlSequenceNode materialsNode;
            var materials = (mainNode.TryGet<YamlSequenceNode>(out materialsNode, "materials")
                    ? materialsNode.Children.Cast<YamlMappingNode>()
                        .Select(a => 
                            {
                                TypeInfo type = null;
                                TypeInfos.TryGetValue(a.GetScalar<int>("typeID"), out type);
                                return Tuple.Create(type, a.GetScalar<int>("quantity"));
                            }
                        )
                        .Where(a => null != a.Item1)
                        .ToDictionary(a => a.Item1, a => a.Item2)
                    : new Dictionary<TypeInfo, int>())
                .ToImmutableDictionary();

            YamlSequenceNode skillsNode;
            var requirements = (mainNode.TryGet<YamlSequenceNode>(out skillsNode, "skills")
                    ? skillsNode.Children.Cast<YamlMappingNode>().Select(a => new Skill.Requirement(Skills[a.GetScalar<int>("typeID")], a.GetScalar<int>("level")))
                    : Enumerable.Empty<Skill.Requirement>())
                .ToImmutableList();

            var time = TimeSpan.FromSeconds(mainNode.GetScalar<int>("time"));

            return new BlueprintTemplate.ResearchDetails(time, requirements, materials);
        }
    }
}
