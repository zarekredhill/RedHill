using System;
using System.Collections.Immutable;
using RedHill.Core.Util;
using RedHill.Core.PlanetaryInteraction;
using YamlDotNet;
using YamlDotNet.RepresentationModel;
using NLog;
using System.Collections.Generic;

namespace RedHill.Core.ESI
{

    public class PlanetarySchematicsProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private StaticFileProvider StaticFileProvider { get; }

        private ImmutableDictionary<int, PlanetarySchematic> Cache { get; set; }

        public PlanetarySchematicsProvider(StaticFileProvider staticFileProvider)
        {
            StaticFileProvider = staticFileProvider;
        }

        public ImmutableDictionary<int, PlanetarySchematic> Get()
        {
            lock (this)
            {
                if (null != Cache) return Cache;
                return Cache = GetData();
            }
        }

        private ImmutableDictionary<int, PlanetarySchematic> GetData()
        {
            var schematicsFile = StaticFileProvider.Get("sde", "bsd", "planetSchematics.yaml");
            var data = Parse(schematicsFile);
            Log.Info("Static data loaded. ({0})", data.Count);
            return data;
        }

        private ImmutableDictionary<int, PlanetarySchematic> Parse(YamlDocument yamlDocument)
        {
            var dict = new Dictionary<int, PlanetarySchematic>();
            foreach (var schematicsNode in ((YamlSequenceNode)yamlDocument.RootNode).Children)
            {
                var staticData = Parse((YamlMappingNode)schematicsNode);
                dict[staticData.Id] = staticData;
            }

            return dict.ToImmutableDictionary();
        }

        private static PlanetarySchematic Parse(YamlMappingNode schematicsNode)
        {
            var cycleTime = TimeSpan.FromSeconds(schematicsNode.GetScalar<int>("cycleTime"));
            var id = schematicsNode.GetScalar<int>("schematicID");
            var name = schematicsNode.GetScalar("schematicName");
            return new PlanetarySchematic(id, name, cycleTime);
        }
    }

}