using System;
using Xunit;
using RedHill.Core;

namespace RedHill.CoreTests
{
    public class ESIRequestTests
    {
        [Fact]
        public async void Trivial()
        {
            var req = new ESIRequest();
            var response = await req.GetResponse();
            Console.WriteLine(response);
            Assert.NotNull(response);
        }
    }
}
