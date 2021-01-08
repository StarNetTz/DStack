using Microsoft.Extensions.Logging;

namespace DStack.Projections.EventStoreDB
{
    public class ESSubscriptionFactory : ISubscriptionFactory
    {
        ILoggerFactory LoggerFactory;

        public ESSubscriptionFactory(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }
        public ISubscription Create()
        {
            return new ESSubscription(LoggerFactory.CreateLogger<ESSubscription>());
        }
    }
}