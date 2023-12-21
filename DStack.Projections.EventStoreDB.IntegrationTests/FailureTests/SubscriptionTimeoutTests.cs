using EventStore.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.EventStoreDB.IntegrationTests;

public class SubscriptionTimeoutTests
{
    ESSubscription Subscription;

    public SubscriptionTimeoutTests()
    {
       // new ESDataGenerator().WriteTestEventsToStore(20000).Wait();
    }

        async Task EventAppeared(object ev, ulong checkpoint)
        {
            await Task.Delay(1000);
        }

    [Fact]
    public async Task Should_fail_after_max_resubscriptions()
    {
        Subscription = new ESSubscription(new NullLoggerFactory().CreateLogger<ESSubscription>(), EventStoreClientFactory.CreateEventStoreClient())
        {
            StreamName = TestProjection.StreamName,
            EventAppearedCallback = EventAppeared
        };
        await Subscription.StartAsync(0);
        await Task.Delay(60000);

        Assert.True(Subscription.HasFailed);
    }
}
