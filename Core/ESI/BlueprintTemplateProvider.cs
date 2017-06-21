using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using NLog;

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
            var activities = (YamlMappingNode) valueNode.Children.Single(a => string.Equals("activities", ((YamlScalarNode)a.Key).Value, StringComparison.OrdinalIgnoreCase)).Value;
            var copying = (YamlMappingNode) activities.Children.SingleOrDefault(a => string.Equals("copying", ((YamlScalarNode)a.Key).Value, StringComparison.OrdinalIgnoreCase)).Value;
            var time = (YamlScalarNode) copying?.Children.Single(a => string.Equals("time", ((YamlScalarNode)a.Key).Value, StringComparison.OrdinalIgnoreCase)).Value;
            TimeSpan? copyTime = null;
            if (null != time) copyTime = TimeSpan.FromSeconds(int.Parse(time.Value));

            return new BlueprintTemplate(type, copyTime);
        }
    }
}