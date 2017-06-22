using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using NLog;
using RedHill.Core.Util;

namespace RedHill.Core.ESI
{
    public class BlueprintTemplateProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private ImmutableDictionary<int, StaticTypeData> TypeData { get; }
        public ImmutableDictionary<StaticTypeData, BlueprintTemplate> Data { get; }

        public BlueprintTemplateProvider(StaticTypeDataProvider StaticTypeDataProvider, StaticFileProvider staticFileProvider)
        {
            TypeData = StaticTypeDataProvider.Data;
            Data = Parse(staticFileProvider.Get("sde", "fsd", "blueprints.yaml"));
            Log.Info("Blueprint templates loaded ({0}).", Data.Count);
        }

        private ImmutableDictionary<StaticTypeData, BlueprintTemplate> Parse(YamlDocument yamlDocument)
        {
            var dict = new Dictionary<StaticTypeData, BlueprintTemplate>();
            foreach (var kvp in ((YamlMappingNode)yamlDocument.RootNode).Children)
            {
                var keyNode = (YamlScalarNode)kvp.Key;
                var id = int.Parse(keyNode.Value);
                StaticTypeData type;
                if (!TypeData.TryGetValue(id, out type)) continue;

                BlueprintTemplate blueprint;
                if (!TryParse(type, keyNode, (YamlMappingNode)kvp.Value, out blueprint)) continue;
                dict[blueprint.Type] = blueprint;
            }
            return dict.ToImmutableDictionary();
        }

        private bool TryParse(StaticTypeData type, YamlScalarNode keyNode, YamlMappingNode valueNode, out BlueprintTemplate result)
        {
            result = null;
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
            if (!valueNode.TryGet<YamlMappingNode>(out manufacturingNode, "activities", "manufacturing")) return false;
            BlueprintTemplate.ManufacturingDetails manufacturing;
            if (!TryGetManufacturingDetails(manufacturingNode, out manufacturing)) return false;

            result = new BlueprintTemplate(type, maxProductionLimit, copyTime, materialResearchTime, timeResearchTime, manufacturing);
            return true;
        }

        private bool TryGetManufacturingDetails(YamlMappingNode node, out BlueprintTemplate.ManufacturingDetails result)
        {
            result = null;
            var time = TimeSpan.FromSeconds(node.GetScalar<int>("time"));
            YamlSequenceNode productsNode;
            if (!node.TryGet<YamlSequenceNode>(out productsNode, "products")) return false;
            var productNode = productsNode.Single();
            StaticTypeData product;
            if (!TypeData.TryGetValue(productNode.GetScalar<int>("typeID"), out product)) return false;
            
            var quantity = productNode.GetScalar<int>("quantity");

            YamlSequenceNode skillsNode;
            var skillReqs = (node.TryGet<YamlSequenceNode>(out skillsNode, "skills")
                    ? skillsNode.Select(a => new Skill.Requirement(null, a.GetScalar<int>("level")))
                    : Enumerable.Empty<Skill.Requirement>())
                .ToImmutableList();

            result = new BlueprintTemplate.ManufacturingDetails(time, product, quantity, skillReqs );
            return true;
        }
    }
}