using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using RedHill.Core.ESI;
using RedHill.Core;

namespace RedHill.CoreTests.ESI
{
    public class BlueprintTemplateProviderTests
    {
        private IServiceProvider Services { get; }
        public BlueprintTemplateProviderTests()
        {
            Services = new ServiceCollection()
                .InstallRedHillCore()
                .BuildServiceProvider();
        }

        [Fact]
        public async void TestParse()
        {
            var dataProvider = Services.GetService<BlueprintTemplateProvider>();
            var blueprints = await dataProvider.Get();
            Assert.NotEmpty(blueprints);
        }
    }
}