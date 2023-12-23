using Microsoft.Extensions.DependencyInjection;
using Starnet;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace DStack.Projections.Testing;

public abstract class ProjectionSpecificationBase<TProjection>
{
    private IProjectionsFactory ProjectionsFactory;

    public IServiceCollection ServiceCollection { get; set; }

    public IProjectionsStore ProjectionsStore { get; set; }

    internal ServiceProvider ServiceProvider { get; set; }

    protected abstract void ConfigureContainer(IServiceCollection services);

    public ProjectionSpecificationBase()
    {
        ServiceCollection = new ServiceCollection();
        ServiceCollection.AddTransient(GetHandlerType());
        ServiceCollection.AddTransient<ICheckpointReader, StubCheckpointReader>();
        ServiceCollection.AddTransient<ICheckpointWriter, StubCheckpointWriter>();
        ServiceCollection.AddTransient<IHandlerFactory, DIHandlerFactory>();
        ServiceCollection.AddTransient<ISubscriptionFactory, InMemorySubscriptionFactory>();
        ServiceCollection.AddTransient<IProjectionsFactory, ProjectionsFactory>();
        ConfigureContainer(ServiceCollection);
        ServiceProvider = ServiceCollection.BuildServiceProvider();
        ServiceCollection.AddSingleton((IServiceProvider)ServiceProvider);
        ProjectionsFactory = ServiceProvider.GetRequiredService<IProjectionsFactory>();
    }

        static Type GetHandlerType()
        {
            Type typeFromHandle = typeof(TProjection);
            Assembly assembly = typeFromHandle.Assembly;
            string name = typeFromHandle.FullName + "Handler";
            return assembly.GetType(name, throwOnError: true, ignoreCase: false);
        }

    public async Task Given(params object[] args)
    {
        IProjection projection = await ProjectionsFactory.CreateAsync<TProjection>().ConfigureAwait(continueOnCapturedContext: false);
        InMemorySubscription inMemorySubscription = projection.Subscription as InMemorySubscription;
        inMemorySubscription.LoadEvents(args);
        await projection.StartAsync().ConfigureAwait(continueOnCapturedContext: false);
    }

    public async Task Expect<TModel>(TModel model) where TModel : class
    {
        string id = ExtractIdFromObject(model);
        string text = ObjectComparer.FindDifferences(model, await ProjectionsStore.LoadAsync<TModel>(id).ConfigureAwait(continueOnCapturedContext: false));
        if (!string.IsNullOrEmpty(text))
            throw new XunitException(text);
    }

        static string ExtractIdFromObject(object model)
        {
            object value = model.GetType().GetProperty("Id")!.GetValue(model, null);
            IdValidator.ValidateType(value);
            return value.ToString();
        }
}
