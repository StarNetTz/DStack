using Microsoft.Extensions.DependencyInjection;

namespace DStack.Projections.Testing;

public class ProjectionSpecification<TProjection> : ProjectionSpecificationBase<TProjection>
where TProjection : class, IProjection
{
    protected override void ConfigureContainer(IServiceCollection services)
    {
        var store = new InMemoryProjectionsStore();
        ServiceCollection.AddSingleton<INoSqlStore>(store);
        ServiceCollection.AddSingleton<ISqlStore>(store);
        
    }

    public ProjectionSpecification():base()
    {
        ProjectionsStore = ServiceProvider.GetRequiredService<INoSqlStore>();
    }

}

public class ProjectionSpecification<TProjection, TProjectionStore> : ProjectionSpecificationBase<TProjection>
where TProjection : class, IProjection
where TProjectionStore : IProjectionsStore
{

    protected override void ConfigureContainer(IServiceCollection services)
    {
    }

    public ProjectionSpecification():base()
    {

        ProjectionsStore = ServiceProvider.GetRequiredService<TProjectionStore>();
    }
}