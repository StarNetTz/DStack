using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.EventStoreDB.IntegrationTests;

public class TransientClientCodeFailureTests
{
    ESSubscription Subscription;
    int Counter = 0;
    ulong CurrentCheckpoint = 0;

    Task EventAppeared(object ev, ulong checkpoint)
    {
        Counter++;
        if (Counter == 3)
        {
            throw new ApplicationException("Transient bug in client code!");
        }
        CurrentCheckpoint = checkpoint;
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Should_resubscribe_and_resume()
    {
        Subscription = new ESSubscription(new NullLoggerFactory().CreateLogger<ESSubscription>(), EventStoreClientFactory.CreateEventStoreClient())
        {
            StreamName = TestProjection.StreamName,
            EventAppearedCallback = EventAppeared
        };
        await Subscription.StartAsync(0);
        await Task.Delay(500);

        Assert.False(Subscription.HasFailed);
        Assert.True(CurrentCheckpoint > 3);
    }
}
