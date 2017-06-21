using System.Linq;
using System.IO;
using YamlDotNet.RepresentationModel;
using Microsoft.Extensions.Options;
using NLog;

namespace RedHill.Core.ESI
{
    public class StaticFileProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        public StaticFileProviderOptions StaticFileProviderOptions { get; }

        public StaticFileProvider(IOptions<StaticFileProviderOptions> staticFileProviderOptions)
        {
            StaticFileProviderOptions = staticFileProviderOptions.Value;
        }

        public YamlDocument Get(params string[] file)
        {
            var s = new string[] {StaticFileProviderOptions.BaseFolder}.Concat(file).ToArray();
            var filePath = Path.Combine(s);
            Log.Info("Loading YAML file {0}", filePath);
            var yaml = new YamlStream();
            yaml.Load(new StreamReader(File.OpenRead(filePath)));
            Log.Info("YAML file {0} loaded.", filePath);
            return yaml.Documents[0];            
        }
    }
}