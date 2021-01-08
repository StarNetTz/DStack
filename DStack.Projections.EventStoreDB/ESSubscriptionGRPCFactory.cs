using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DStack.Projections.EventStoreDB
{
    public class ESSubscriptionGRPCFactory : ISubscriptionFactory
    {
        ILoggerFactory LoggerFactory;
        EventStoreClient Client;

        public ESSubscriptionGRPCFactory(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            LoggerFactory = loggerFactory;
            var settings = EventStoreClientSettings.Create(configuration["EventStoreDB:ConnectionString"]);
            Client = new EventStoreClient(settings);
        }
        public ISubscription Create()
        {
            return new ESSubscriptiongRPC(LoggerFactory.CreateLogger<ESSubscription>(), Client);
        }
    }
}