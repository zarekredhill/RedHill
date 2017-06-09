using System;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedHill.Core.ESI
{
    public class SkillsProvider
    {
        private RequestHandler RequestHandler { get; }

        public SkillsProvider(RequestHandler requestHandler)
        {
            RequestHandler = requestHandler;
        }

        public async Task<ImmutableList<Skill>> GetSkills()
        {
            var response = await RequestHandler.GetResponse("universe/categories");
            var content = await response.Content.ReadAsStringAsync();

            var foo = (JArray) JsonConvert.DeserializeObject(content);

            var result = foo.Select(a => new Skill(a.ToString(), a.ToString())).ToImmutableList();
            return result;
        }

    }
}