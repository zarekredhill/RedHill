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
            foreach(var kvp in ((YamlMappingNode) yamlDocument.RootNode).Children)
            {
                var keyNode = (YamlScalarNode)kvp.Key;
                var id = int.Parse(keyNode.Value);
                StaticTypeData type;
                if (!TypeData.TryGetValue(id, out type)) continue;

                var item = Parse(type, keyNode, (YamlMappingNode)kvp.Value);
                dict[item.Type] = item;
            }
            return dict.ToImmutableDictionary();
        }

        private BlueprintTemplate Parse(StaticTypeData type, YamlScalarNode keyNode, YamlMappingNode valueNode) 
        {
            int copyTimeSecond;

            var times = new[]{ "copying", "research_material", "research_time" }
                .Select( a => valueNode.TryGetScalar<int>(out copyTimeSecond, "activities", a, "time")
                ? (TimeSpan?)TimeSpan.FromSeconds(copyTimeSecond) 
                : null).ToArray();
            
            var copyTime = times[0];
            var materialResearchTime = times[1];
            var timeResearchTime = times[2];

            var maxProductionLimit = valueNode.GetScalar<int>("maxProductionLimit");
            return new BlueprintTemplate(type, maxProductionLimit, copyTime, materialResearchTime, timeResearchTime);
        }
    }
}