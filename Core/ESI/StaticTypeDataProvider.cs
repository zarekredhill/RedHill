using System;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;
using YamlDotNet;
using YamlDotNet.RepresentationModel;
using NLog;
using RedHill.Core.Util;

namespace RedHill.Core.ESI
{
    public class StaticTypeDataProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public ImmutableDictionary<int, StaticTypeData> Data { get; }

        public StaticTypeDataProvider(StaticFileProvider staticFileProvider)
        {
            Data = Parse(staticFileProvider.Get("sde", "fsd", "typeIDs.yaml"));
            Log.Info("Static type data loaded. ({0})", Data.Count);
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
            
            string name;
            if (!valueNode.TryGetScalar(out name, "name", "en")) return new StaticTypeData(id, null, null);
            string description;
            if (!valueNode.TryGetScalar(out description, "description", "en")) description = name;

            return new StaticTypeData(id, name, description);
        }
    }
}