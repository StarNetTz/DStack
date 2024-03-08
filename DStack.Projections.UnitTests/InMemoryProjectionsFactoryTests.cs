using System.Reflection;
using System.Threading.Tasks;
using DStack.Projections.Testing;
using Microsoft.Extensions.DependencyInjection;
using DStack.Projections.UnitTests;
using Xunit;

namespace DStack.Projections.Tests;

public class InMemoryProjectionsFactoryTests
{
    IProjectionsFactory ProjectionsFactory;

    public InMemoryProjectionsFactoryTests()
    {
        var ServiceCollection = new ServiceCollection();

        ServiceCollection.AddSingleton<ICustomStore, CustomStore>();

        ServiceCollection.AddSingleton<INoSqlStore, InMemoryProjectionsStore>();
        ServiceCollection.AddSingleton<ISqlStore, InMemoryProjectionsStore>();
        ServiceCollection.AddTransient<ICheckpointReader, StubCheckpointReader>();
        ServiceCollection.AddTransient<ICheckpointWriter, StubCheckpointWriter>();

        ServiceCollection.AddTransient<IHandlerFactory, DIHandlerFactory>();
        ServiceCollection.AddTransient<ISubscriptionFactory, InMemorySubscriptionFactory>();
        ServiceCollection.AddTransient<IProjectionsFactory, ProjectionsFactory>();
        ServiceCollection.AddTransient<ITimeProvider, MockTimeProvider>();
        ServiceCollection.AddTransient<FailingHandler>();
        ServiceCollection.AddTransient<TestProjectionHandler>();
        ServiceCollection.AddTransient<TestProjectionWithCustomStoreHandler>();

        var provider = ServiceCollection.BuildServiceProvider();
        ProjectionsFactory = provider.GetRequiredService<IProjectionsFactory>();
    }

    [Fact]
    public async Task can_create_test_projection_and_project()
    {
        var proj = await ProjectionsFactory.CreateAsync<TestProjection>();
        PreloadProjectionsSubscription(proj);

        await proj.StartAsync();

        Assert.Equal(2UL, proj.Checkpoint.Value);
    }

        void PreloadProjectionsSubscription(IProjection proj)
        {
            var sub = proj.Subscription as InMemorySubscription;
            sub.LoadEvents(
               new TestEvent() { Id = "1", SomeValue = "Manchester - Sloboda" },
               new TestEvent() { Id = "2", SomeValue = "Manchester - Sloboda City" }
               );
        }

    [Fact]
    public async Task failing_projection_throws_an_aggregate_exception()
    {
        var proj = await ProjectionsFactory.CreateAsync<FailingProjection>();
        PreloadFailingProjectionsSubscription(proj);
        await Assert.ThrowsAsync<ProjectionException>(proj.StartAsync);
    }

        void PreloadFailingProjectionsSubscription(IProjection proj)
        {
            var sub = proj.Subscription as InMemorySubscription;
            sub.LoadEvents(
               new TestEvent() { Id = "1", SomeValue = "Manchester - Sloboda" },
               new TestEvent() { Id = "2", SomeValue = "Manchester - Sloboda City" }
               );
        }

    [Fact]
    public async Task can_create_all_projections_within_assembly()
    {
        var projections = await ProjectionsFactory.CreateAsync(Assembly.GetAssembly(typeof(TestProjection)));
        Assert.Equal(3, projections.Count);
    }
}