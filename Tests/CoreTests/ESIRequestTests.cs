using System;
using Microsoft.Extensions.Logging;
using Xunit;
using RedHill.Core;

namespace RedHill.CoreTests
{

    public class MyFixture : IDisposable
    {
        private ILoggerFactory LogFactory { get; }
        public ESIRequestHandler ESIRequestHandler { get; }

        public MyFixture()
        {
            LogFactory = new LoggerFactory();
            ESIRequestHandler = new ESIRequestHandler("https://esi.tech.ccp.is", "latest", "tranquility", LogFactory.CreateLogger(typeof(ESIRequestHandler)));
        }

        void IDisposable.Dispose()
        {
            LogFactory.Dispose();
        }
    }
    
    [CollectionDefinition(nameof(ESIRequestTests))]
    public class MyCollectionFixture : ICollectionFixture<MyFixture>
    {

    }

    [Collection(nameof(ESIRequestTests))]
    public class ESIRequestTests : IClassFixture<MyFixture>
    {
        private readonly MyFixture _fixture;
      public ESIRequestTests(MyFixture fixture)
      {
        _fixture = fixture;
      }

        [Fact]
        public async void Trivial()
        {
            var response = await _fixture.ESIRequestHandler.GetResponse("universe/categories");
            Assert.NotNull(response);
        }
    }
}
