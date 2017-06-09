using System;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using NLog;

namespace RedHill.Core.ESI
{
    public class DataProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private SkillsProvider Skills { get; }
        private MemoryCache Cache { get; } = new MemoryCache(new MemoryCacheOptions());

        public DataProvider(SkillsProvider skills)
        {
            Skills = skills;
        }

        public async Task<ImmutableList<Skill>> GetSkills()
        {
            ImmutableList<Skill> result;
            if (!Cache.TryGetValue<ImmutableList<Skill>>("skills", out result))
            {
                Log.Info("Cache miss: skills.");
                Cache.Set("skills", result = await Skills.GetSkills(), TimeSpan.FromHours(1));
            }
            return result;
        }

    }
}