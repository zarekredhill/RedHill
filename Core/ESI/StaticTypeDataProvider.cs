using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;
using YamlDotNet;
using YamlDotNet.RepresentationModel;
using NLog;

namespace RedHill.Core.ESI
{
    public class StaticTypeDataProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public ImmutableDictionary<int, StaticTypeData> StaticTypeData { get; }

        public StaticTypeDataProvider(StaticFileProvider staticFileProvider)
        {
            StaticTypeData = Parse(staticFileProvider.Get("sde", "fsd", "typeIDs.yaml"));
        }

        private ImmutableDictionary<int, StaticTypeData> Parse(YamlDocument yamlDocument)
        {
            var dict = new Dictionary<int, StaticTypeData>();
            foreach(var kvp in ((YamlMappingNode) yamlDocument.RootNode).Children)
            {
                var staticData = Parse((YamlScalarNode)kvp.Key, (YamlMappingNode)kvp.Value);
                dict[staticData.Id] = staticData;
            }

            return dict.ToImmutableDictionary();
        }

        private static StaticTypeData Parse(YamlScalarNode keyNode, YamlMappingNode valueNode) 
        {
            var id = int.Parse(keyNode.Value);
            
            var nameNode = (YamlMappingNode) valueNode.Children.Single(a => string.Equals("name", ((YamlScalarNode)a.Key).Value, StringComparison.OrdinalIgnoreCase)).Value;
            if (!nameNode.Any()) return new StaticTypeData(id, null, null);
            var name = ((YamlScalarNode) nameNode.Children.Single(a => string.Equals("en", ((YamlScalarNode)a.Key).Value, StringComparison.OrdinalIgnoreCase)).Value).Value;

            var descriptions = (YamlMappingNode) valueNode.Children.SingleOrDefault(a => string.Equals("description", ((YamlScalarNode)a.Key).Value, StringComparison.OrdinalIgnoreCase)).Value;
            var description = ((YamlScalarNode) descriptions?.Children?.SingleOrDefault(a => string.Equals("en", ((YamlScalarNode)a.Key).Value, StringComparison.OrdinalIgnoreCase)).Value)?.Value
                ?? name;

            return new StaticTypeData(id, name, description);
        }
    }
}