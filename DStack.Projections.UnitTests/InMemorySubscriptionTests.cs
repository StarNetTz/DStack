﻿using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.Tests;

public class InMemorySubscriptionTests
{
    InMemorySubscription Subscription;

    ulong Checkpoint = 0;
    object LastEvent = null;

    private Task EventAppeared(object ev, ulong checkpoint)
    {
        Checkpoint = checkpoint;
        LastEvent = ev;
        return Task.CompletedTask;
    }

    public InMemorySubscriptionTests()
    {
        LastEvent = null;
        Subscription = new InMemorySubscription() { Name = "MySubscription", StreamName = "myStream", EventAppearedCallback = EventAppeared };
    }


    [Fact]
    public async Task can_project()
    {
        Subscription.LoadEvents(new TestEvent() { Id = "1", SomeValue = "Manchester - Sloboda" });
        await Subscription.StartAsync(0);
        AssertThatEventProjectedAsExpected();
    }

        void AssertThatEventProjectedAsExpected()
        {
            var e = LastEvent as TestEvent;
            Assert.Equal(1UL, Checkpoint);
            Assert.Equal("1", e.Id);
            Assert.Equal("Manchester - Sloboda", e.SomeValue);
        }

    [Fact]
    public async Task can_project_multiple_events()
    {
        LoadTwoEvents();
        await Subscription.StartAsync(0);
        AssertLastEvent();
    }

    [Fact]
    public async Task can_read_stream_from_given_checkpoint_of_2()
    {
        LoadTwoEvents();
        await Subscription.StartAsync(2);
        AssertLastEvent();
    }

    void LoadTwoEvents()
    {
        Subscription.LoadEvents(
            new TestEvent() { Id = "1", SomeValue = "Manchester - Sloboda" },
            new TestEvent() { Id = "2", SomeValue = "Manchester - Sloboda City" }
            );
    }

    void AssertLastEvent()
    {
        var e = LastEvent as TestEvent;
        Assert.Equal(2UL, Checkpoint);
        Assert.Equal("2", e.Id);
        Assert.Equal("Manchester - Sloboda City", e.SomeValue);
    }
}