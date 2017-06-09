using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using RedHill.Core.ESI;
using RedHill.Core;

namespace RedHill.CoreTests.ESI
{
    public class CachingDataProviderTests
    {
        private IServiceProvider Services { get; }
        public CachingDataProviderTests()
        {
            Services = new ServiceCollection()
                .InstallRedHillCore()
                .BuildServiceProvider();
        }

        [Fact]
        public void Trivial()
        {
            var dataProvider = Services.GetService<CachingDataProvider>();
            Assert.NotNull(dataProvider);
        }

        [Fact]
        public async void TestSomeSkillsExist()
        {
            var dataProvider = Services.GetService<CachingDataProvider>();
            var skills = await dataProvider.GetSkills();
            Assert.NotEmpty(skills);
        }

    }
}