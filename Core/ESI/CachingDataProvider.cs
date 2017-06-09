using System;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using NLog;

namespace RedHill.Core.ESI
{
    public class CachingDataProvider
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IDistributedCache Cache { get; }
        private DataProvider DataProvider { get; }

        public CachingDataProvider(IDistributedCache cache, DataProvider dataProvider)
        {
            Cache = cache;
            DataProvider = dataProvider;
        }

        public async Task<ImmutableList<Skill>> GetSkills()
        {
            var result = await DataProvider.GetSkills();
            return result;
        }
    }
}