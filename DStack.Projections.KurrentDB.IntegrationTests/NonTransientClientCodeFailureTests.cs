using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.KurrentDB.IntegrationTests;

public class NonTransientClientCodeFailureTests
{
    KurrentSubscription Subscription;

    Task EventAppeared(object ev, ulong checkpoint)
    {
        throw new ApplicationException("Bug in client code!");
    }

    [Fact]
    public async Task Should_fail_after_max_resubscriptions()
    {
        Subscription = new KurrentSubscription(NullLogger<KurrentSubscription>.Instance, KurrentDBClientFactory.CreateKurrentDBClient())
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
