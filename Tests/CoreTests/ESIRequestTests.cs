using System;
using Microsoft.Extensions.Logging;
using Xunit;
using RedHill.Core;

namespace RedHill.CoreTests
{   
    [CollectionDefinition(nameof(ESIRequestTests))]
    public class MyCollectionFixture : ICollectionFixture<ESIRequestTests.Fixture>
    {
    }

    [Collection(nameof(ESIRequestTests))]
    public class ESIRequestTests : IClassFixture<ESIRequestTests.Fixture>
    {
        public class Fixture: IDisposable
        {
            private ILoggerFactory LogFactory { get; }
            public ESIRequestHandler ESIRequestHandler { get; }

            public Fixture()
            {
                LogFactory = new LoggerFactory();
                ESIRequestHandler = new ESIRequestHandler("https://esi.tech.ccp.is", "latest", "tranquility", LogFactory.CreateLogger(typeof(ESIRequestHandler)));
            }

            void IDisposable.Dispose()
            {
                LogFactory.Dispose();
            }
        }
        private readonly Fixture _fixture;
      public ESIRequestTests(Fixture fixture)
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
