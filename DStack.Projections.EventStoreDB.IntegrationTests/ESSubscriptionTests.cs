﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.EventStoreDB.IntegrationTests;

public class ESSubscriptionTests
{
    ESSubscription Subscription;

    ulong Checkpoint = 0;
    object LastEvent = null;

    public ESSubscriptionTests()
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
    public async Task Should_Subscribe_And_Receive_Events()
    {
        Subscription = new ESSubscription(new NullLoggerFactory().CreateLogger<ESSubscription>(), EventStoreClientFactory.CreateEventStoreClient())
        {
            Name = nameof(TestProjection),
            StreamName = TestProjection.StreamName,
            EventAppearedCallback = EventAppeared
        };

        _= Subscription.StartAsync(0);
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
