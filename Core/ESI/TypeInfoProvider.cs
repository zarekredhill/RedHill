using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Collections.Generic;
using YamlDotNet;
using YamlDotNet.RepresentationModel;
using NLog;
using RedHill.Core.Util;

namespace RedHill.Core.ESI
{
    public class TypeInfoProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private StaticFileProvider StaticFileProvider { get; }
        private ImmutableDictionary<int, TypeInfo> Cache { get; set; }

        public TypeInfoProvider(StaticFileProvider staticFileProvider)
        {
            StaticFileProvider = staticFileProvider;
        }

        public ImmutableDictionary<int, TypeInfo> Get()
        {
            lock (this)
            {
                if (null != Cache) return Cache;
                return Cache = GetData();
            }
        }

        private ImmutableDictionary<int, TypeInfo> GetData()
        {
            var data = Parse(StaticFileProvider.Get("sde", "fsd", "typeIDs.yaml"));
            Log.Info("Static type data loaded. ({0})", data.Count);
            return data;
        }

        private ImmutableDictionary<int, TypeInfo> Parse(YamlDocument yamlDocument)
        {
            var dict = new Dictionary<int, TypeInfo>();
            foreach (var kvp in ((YamlMappingNode)yamlDocument.RootNode).Children)
            {
                var staticData = Parse((YamlScalarNode)kvp.Key, (YamlMappingNode)kvp.Value);
                dict[staticData.Id] = staticData;
            }

            return dict.ToImmutableDictionary();
        }

        private static TypeInfo Parse(YamlScalarNode keyNode, YamlMappingNode valueNode)
        {
            var id = int.Parse(keyNode.Value);

            string name;
            if (!valueNode.TryGetScalar(out name, "name", "en")) return new TypeInfo(id, null, null);
            string description;
            if (!valueNode.TryGetScalar(out description, "description", "en")) description = name;

            return new TypeInfo(id, name, description);
        }
    }
}