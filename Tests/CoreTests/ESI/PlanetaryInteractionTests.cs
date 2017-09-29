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
        public void Testschematics()
        {
            var dataProvider = Services.GetService<PlanetarySchematicsProvider>();
            var skills = dataProvider.Get();
            Assert.NotEmpty(skills);
        }
    }
}