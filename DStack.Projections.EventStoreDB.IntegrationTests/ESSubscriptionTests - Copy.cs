using EventStore.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.EventStoreDB.IntegrationTests;

public class ESSubscriptionTests2
{
    ESSubscription2 Subscription;

    ulong Checkpoint = 0;
    object LastEvent = null;

    public ESSubscriptionTests2()
    {
        Checkpoint = 0;
        LastEvent = null;
    }

        Task EventAppeared(object ev, ulong checkpoint)
        {
            Checkpoint = checkpoint;
            LastEvent = ev;
            return Task.CompletedTask;
        }

    [Fact]
    public async Task Should_Subscribe_And_Recieve_Events()
    {
        Subscription = new ESSubscription2(new NullLoggerFactory().CreateLogger<ESSubscription>(), EventStoreClientFactory.CreateEventStoreClient())
        {
            Name = nameof(TestProjection),
            StreamName = TestProjection.StreamName,
            EventAppearedCallback = EventAppeared
        };

        await Subscription.StartAsync(0);
        await Task.Delay(200);

        AssertThatEventsProjected();
    }


        void AssertThatEventsProjected()
        {
            Assert.IsType<TestEvent>(LastEvent);
            Assert.NotNull(LastEvent);
            Assert.Contains("A guid:", (LastEvent as TestEvent).SomeValue);
            Assert.True(Checkpoint > 0);
        }
}
