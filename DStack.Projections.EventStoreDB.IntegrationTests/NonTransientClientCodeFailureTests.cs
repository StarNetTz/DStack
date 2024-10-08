﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.EventStoreDB.IntegrationTests;

public class NonTransientClientCodeFailureTests
{
    ESSubscription Subscription;
  
    Task EventAppeared(object ev, ulong checkpoint)
    {
        throw new ApplicationException("Bug in client code!");
    }

    [Fact]
    public async Task Should_fail_after_max_resubscriptions()
    {
        Subscription = new ESSubscription(new NullLoggerFactory().CreateLogger<ESSubscription>(), EventStoreClientFactory.CreateEventStoreClient())
        {
            Name = nameof(TestProjection),
            StreamName = TestProjection.StreamName,
            EventAppearedCallback = EventAppeared
        };
        _= Subscription.StartAsync(0);
        await Task.Delay(500);

        Assert.True(Subscription.HasFailed);
        Assert.Equal("Bug in client code!", Subscription.Error);
    }
}
