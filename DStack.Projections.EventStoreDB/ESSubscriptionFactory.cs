using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DStack.Projections.EventStoreDB;

public class ESSubscriptionFactory : ISubscriptionFactory
{
    ILoggerFactory LoggerFactory;
    EventStoreClient Client;

    public ESSubscriptionFactory(ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        LoggerFactory = loggerFactory;
        var settings = EventStoreClientSettings.Create(configuration["EventStoreDB:ConnectionString"]);
        Client = new EventStoreClient(settings);
    }
    public ISubscription Create()
    {
        return new ESSubscription3(LoggerFactory.CreateLogger<ESSubscription3>(), Client);
    }
}