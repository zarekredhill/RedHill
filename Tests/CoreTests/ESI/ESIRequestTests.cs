using System;
using Xunit;
using RedHill.Core.ESI;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace RedHill.CoreTests.ESI
{   
    [CollectionDefinition(nameof(ESIRequestTests))]
    public class MyCollectionFixture : ICollectionFixture<ESIRequestTests.Fixture>
    {
        public MyCollectionFixture()
        {
                      
        }
    }

    [Collection(nameof(ESIRequestTests))]
    public class ESIRequestTests : IClassFixture<ESIRequestTests.Fixture>
    {
        public class Fixture: IDisposable
        {
            public ESIRequestHandler ESIRequestHandler { get; }

            public Fixture()
            {
                var config = new LoggingConfiguration();
                var t = new FileTarget
                {
                    Name = "main",
                    FileName = "main.log",
                    MaxArchiveFiles=1,
                    ArchiveNumbering=ArchiveNumberingMode.Rolling,
                    ArchiveOldFileOnStartup=true,
                    Layout = "${longdate}|${logger}|${uppercase:${level}}|${message} ${exception}"
                };
                config.AddTarget(t);
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, t));
                LogManager.Configuration = config;
                LogManager.Configuration.Reload();  
                
                ESIRequestHandler = new ESIRequestHandler("https://esi.tech.ccp.is", "latest", "tranquility");
            }

            void IDisposable.Dispose()
            {
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
