using Microsoft.Extensions.DependencyInjection;
using Starnet.ObjectComparer;
using System;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace DStack.Projections.Testing
{
    public class ProjectionSpecification<TProjection>
    where TProjection : class, IProjection
    {
        public IServiceCollection ServiceCollection { get; set; }

        public IProjectionsStore ProjectionsStore { get; set; }
        IProjectionsFactory ProjectionsFactory;

        protected virtual void ConfigureContainer(IServiceCollection services) { }

        public ProjectionSpecification()
        {
            ServiceCollection = new ServiceCollection();
            ServiceCollection.AddTransient(GetHandlerType());
            var store = new InMemoryProjectionsStore();
            ServiceCollection.AddSingleton<INoSqlStore>(store);
            ServiceCollection.AddSingleton<ISqlStore>(store);
            ServiceCollection.AddTransient<ICheckpointReader, StubCheckpointReader>();
            ServiceCollection.AddTransient<ICheckpointWriter, StubCheckpointWriter>();

            ServiceCollection.AddTransient<IHandlerFactory, DIHandlerFactory>();
            ServiceCollection.AddTransient<ISubscriptionFactory, InMemorySubscriptionFactory>();
            ServiceCollection.AddTransient<IProjectionsFactory, ProjectionsFactory>();

            ConfigureContainer(ServiceCollection);

            var provider = ServiceCollection.BuildServiceProvider();
            ServiceCollection.AddSingleton<IServiceProvider>(provider);

            ProjectionsFactory = provider.GetRequiredService<IProjectionsFactory>();
            ProjectionsStore = provider.GetRequiredService<INoSqlStore>();
        }

            static Type GetHandlerType()
            {
                var projType = typeof(TProjection);
                var assem = projType.Assembly;
                var handlerName = projType.FullName + "Handler";
                var handlerType = assem.GetType(handlerName, true, false);
                return handlerType;
            }

        public async Task Given(params object[] args)
        {
            var p = await ProjectionsFactory.Create<TProjection>();
            var s = p.Subscription as InMemorySubscription;
            s.LoadEvents(args);
            await p.Start();
        }

        public async Task Expect<TModel>(TModel model) where TModel : class
        {
            var id = ExtractIdFromObject(model);
            var actual = await ProjectionsStore.LoadAsync<TModel>(id);
            var diff = ObjectComparer.FindDifferences(model, actual);
            if (!string.IsNullOrEmpty(diff))
                throw new XunitException(diff);
        }

            static string ExtractIdFromObject(object model)
            {
                var id = model.GetType().GetProperty("Id").GetValue(model, null);
                ValidateIdType(id);
                return id.ToString();
            }

                static void ValidateIdType(object id)
                {
                    switch (id)
                    {
                        case string s:
                        case int i:
                        case long l:
                        case Guid g:
                            return;
                        default:
                            throw new ArgumentException("Unsopported Id type!");
                    }
                }
    }
}