﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.EventStoreDB.IntegrationTests;

public class ESProjectionsFactoryTests
{
    IProjectionsFactory ProjectionsFactory;

    public ESProjectionsFactoryTests()
    {
       
        var services = CreateServiceCollection();
        var prov = services.BuildServiceProvider();
        ProjectionsFactory = prov.GetRequiredService<IProjectionsFactory>();
        new ESDataGenerator().WriteTestEventsToStore(10).Wait();
    }

        static ServiceCollection CreateServiceCollection()
        {
            var services = new ServiceCollection();
            var configuration = ConfigurationFactory.CreateConfiguration();
            services.AddSingleton(configuration);
            services.AddTransient<IHandlerFactory, StubHandlerFactory>();
            services.AddTransient<ICheckpointReader, StubCheckpointReader>();
            services.AddTransient<ICheckpointWriter, StubCheckpointWriter>();

            services.AddTransient<ISubscriptionFactory, ESSubscriptionFactory>();
            services.AddTransient<IProjectionsFactory, ProjectionsFactory>();
            services.AddLogging(b =>
            {
                b.ClearProviders();
                b.SetMinimumLevel(LogLevel.Information);
            });
            return services;
        }

    [Fact]
    public async Task Should_Project()
    {
        var proj = await ProjectionsFactory.CreateAsync<TestProjection>();
        _= proj.StartAsync();
        await Task.Delay(250);
        Assert.True(proj.Checkpoint.Value > 0);
    }
}