using DStack.Aggregates.HostBuilder;
using DStack.Projections.EventStoreDB;
using DStack.Projections.EventStoreDB.Utils;
using DStack.Projections.RavenDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace DStack.Projections.HostBuilder
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseDStackProjections(
            this IHostBuilder hostBuilder,
            ProjectionsStorageOptions projectionsStorageOptions,
            ProjectionCheckpointsStorageOptions checkpointsStorageOptions,
            params Assembly[] assembliesContainingProjections
            ) 
        {
            hostBuilder.ConfigureServices((ctx, serviceCollection) =>
            {
                serviceCollection.AddTransient<IHandlerFactory, DIHandlerFactory>();
                serviceCollection.AddTransient<IProjectionsFactory, ProjectionsFactory>();

                switch (projectionsStorageOptions)
                {
                    case ProjectionsStorageOptions.EventStoreDB:
                        serviceCollection.AddTransient<ISubscriptionFactory, ESSubscriptionFactory>();
                        serviceCollection.AddTransient<IJSProjectionsFactory, JSProjectionsFactory>();
                        break;
                    default:
                        break;
                }

                switch (checkpointsStorageOptions)
                {
                    case ProjectionCheckpointsStorageOptions.RavenDB:
                        serviceCollection.AddSingleton<INoSqlStore, DefensiveRavenDBProjectionsStore>();
                        serviceCollection.AddSingleton<ISqlStore, DefensiveRavenDBProjectionsStore>();
                        serviceCollection.AddTransient<ICheckpointReader, RavenDBCheckpointReader>();
                        serviceCollection.AddTransient<ICheckpointWriter, RavenDBCheckpointWriter>();
                        break;
                    default:
                        break;
                }

                foreach (var assembly in assembliesContainingProjections)
                {
                    var results = from type in assembly.GetTypes()
                                  where typeof(IHandler).IsAssignableFrom(type)
                                  select type;
                    foreach (var t in results)
                        serviceCollection.AddTransient(t);
                }
            });
            return hostBuilder;
        }
    }
}
