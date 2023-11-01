using Microsoft.Extensions.DependencyInjection;
using DStack.Projections.Testing;
using DStack.Projections.Tests;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace DStack.Projections.UnitTests;

public interface ICustomStore : IProjectionsStore
{

}

public class CustomStore: InMemoryProjectionsStore, ICustomStore
{
}

public class ProjectionSpecificationWithStoreTests : ProjectionSpecification<TestProjectionWithCustomStore, ICustomStore>
{

    protected override void ConfigureContainer(IServiceCollection services)
    {
        services.AddTransient<ITimeProvider, MockTimeProvider>();
        services.AddSingleton<ICustomStore, CustomStore>();
        base.ConfigureContainer(services);
    }

    [Fact]
    public async Task can_project_event()
    {
        var id = $"Company-{Guid.NewGuid()}";
        await Given(new TestEvent() { Id = id, SomeValue = "123" });
        await Expect(new TestModel() { Id = id, SomeValue = "123" });
    }

    [Fact]
    public async Task cannot_project_event_with_unsupported_id_type()
    {
        var id = $"Company-{Guid.NewGuid()}";

        await Assert.ThrowsAsync<ArgumentException>(async () => {
            await Given(new TestEvent() { Id = id, SomeValue = "123" });
            await Expect(new TestModelWithUnsupportedIdType() { Id = 1, SomeValue = "123" });  });
    }


    [Fact]
    public async Task unexpected_result_throws_assertion_exception()
    {
        var id = $"Company-{Guid.NewGuid()}";
        await Assert.ThrowsAsync<XunitException>(async () => {
            await Given(new TestEvent() { Id = id, SomeValue = "123" });
            await Expect(new TestModel() { Id = id, SomeValue = "1234" }); });
    }
}