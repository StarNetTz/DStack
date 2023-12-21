using EventStore.Client;

namespace DStack.Projections.EventStoreDB.IntegrationTests;

public static class EventStoreClientFactory
{
    public static EventStoreClient CreateEventStoreClient()
    {
        var configuration = ConfigurationFactory.CreateConfiguration();
        var settings = EventStoreClientSettings.Create(configuration["EventStoreDB:ConnectionString"]);
        var cli = new EventStoreClient(settings);
        return cli;
    }
}