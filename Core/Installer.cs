using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Extensions.Logging;
using RedHill.Core.ESI;

namespace RedHill.Core
{
    public static class Installers
    {
        public static IServiceCollection InstallRedHillCore(this ServiceCollection services)
        {
            var config = new LoggingConfiguration();
            var t = new FileTarget
            {
                Name = "main",
                FileName = "main.log",
                MaxArchiveFiles = 1,
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                ArchiveOldFileOnStartup = true,
                Layout = "${longdate}|${logger}|${uppercase:${level}}|${message} ${exception}"
            };
            config.AddTarget(t);
            config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Info, t));
            LogManager.Configuration = config;
            LogManager.Configuration.Reload();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("app-settings.json", false)
                .Build();

            services
                .AddSingleton<RequestHandler>()
                .AddSingleton<CategoriesProvider>()
                .AddSingleton<SkillsProvider>()
                .AddSingleton<BlueprintTemplateProvider>()
                .AddSingleton<TypeAttributeProvider>()
                .AddSingleton<AttributesProvider>()
                .AddSingleton<StaticFileProvider>()
                .AddSingleton<TypeInfoProvider>()                
                .AddSingleton<TypeAttributeProvider>()
                .AddSingleton<DataProvider>()
                .AddSingleton<PlanetarySchematicsProvider>()
                .AddSingleton<PlanetaryCommoditiesProvider>()
                .AddDistributedRedisCache(option =>
               {
                   option.Configuration = configuration["RedisSettings:Configuration"];
                   option.InstanceName = configuration["RedisSettings:InstanceName"];
               })
               .Configure<ESIEndpointSettings>(configuration.GetSection("ESIEndpointSettings"))
               .Configure<StaticFileProviderOptions>(configuration.GetSection("StaticFileProviderOptions"));

            return services;
        }
    }
}