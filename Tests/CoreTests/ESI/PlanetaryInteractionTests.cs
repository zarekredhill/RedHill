using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using RedHill.Core.ESI;
using RedHill.Core;

namespace RedHill.CoreTests.ESI
{
    public class PlanetaryInteractionTests
    {
        private IServiceProvider Services { get; }
        public PlanetaryInteractionTests()
        {
            Services = new ServiceCollection()
                .InstallRedHillCore()
                .BuildServiceProvider();
        }

        [Fact]
        public void TestSchematics()
        {
            var dataProvider = Services.GetService<PlanetarySchematicsProvider>();
            var data = dataProvider.Get();
            Assert.NotEmpty(data);
        }

        [Fact]
        public async void TestPlanetaryCommoditiesProvider()
        {
            var dataProvider = Services.GetService<PlanetaryCommoditiesProvider>();
            var data = await dataProvider.Get();
            Assert.NotEmpty(data);
        }
    }
}