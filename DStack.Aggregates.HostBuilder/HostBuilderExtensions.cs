using DStack.Aggregates.EventStoreDB;
using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DStack.Aggregates.HostBuilder
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseDStackAggregates(this IHostBuilder hostBuilder, AggregateStorageOptions storageOptions) 
        {
            hostBuilder.ConfigureServices((ctx, serviceCollection) =>
            {
                switch (storageOptions)
                {
                    case AggregateStorageOptions.EventStoreDB:
                        var esAgg = CreateEventStoreAggregateRepository(ctx.Configuration);
                        serviceCollection.AddSingleton(typeof(IAggregateRepository), esAgg);
                        break;
                    default:
                        break;
                }
            });
            return hostBuilder;
        }

            static ESAggregateRepository CreateEventStoreAggregateRepository(IConfiguration config)
            {
                var settings = EventStoreClientSettings.Create(config["EventStoreDB:ConnectionString"]);
                var client = new EventStoreClient(settings);
                AssertEventStoreAvailable(client);
                return new ESAggregateRepository(client);
            }

            static void AssertEventStoreAvailable(EventStoreClient client)
                => _ = client.GetStreamMetadataAsync("$ce-Any").Result;
    }
}
