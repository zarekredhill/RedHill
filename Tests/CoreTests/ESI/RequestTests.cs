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
            var dataProvider = Services.GetService<DataProvider>();
            var response = await dataProvider.GetSkills();
            Assert.NotNull(response);
        }        
    }
}
