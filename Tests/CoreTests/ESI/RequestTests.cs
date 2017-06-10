using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using RedHill.Core;
using RedHill.Core.ESI;

namespace RedHill.CoreTests.ESI
{
    public class RequestTests 
    {
      
       private IServiceProvider Services { get; }
        public RequestTests()
        {
           Services = new ServiceCollection()
                .InstallRedHillCore()
                .BuildServiceProvider();
        }

        [Fact]
        public async void TestSkills()
        {
            var skills = Services.GetService<SkillsProvider>();
            var response = await skills.GetSkills();
            Assert.NotNull(response);
        }        
    }
}
