using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using RedHill.Core.ESI;
using RedHill.Core;

namespace RedHill.CoreTests.ESI
{
    public class SkillsProviderTests
    {
        private IServiceProvider Services { get; }
        public SkillsProviderTests()
        {
            Services = new ServiceCollection()
                .InstallRedHillCore()
                .BuildServiceProvider();
        }

        [Fact]
        public void Trivial()
        {
            var dataProvider = Services.GetService<SkillsProvider>();
            Assert.NotNull(dataProvider);
        }

        [Fact]
        public async void TestSomeSkillsExist()
        {
            var dataProvider = Services.GetService<SkillsProvider>();
            var skills = await dataProvider.Get();
            Assert.NotEmpty(skills);
        }
    }
}