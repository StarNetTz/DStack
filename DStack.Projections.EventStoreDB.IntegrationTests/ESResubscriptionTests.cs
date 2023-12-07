using EventStore.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.EventStoreDB.IntegrationTests;

public class ESResubscriptionTests
{
    ESSubscription Subscription;

    ulong Checkpoint = 0;
    object LastEvent = null;

    public ESResubscriptionTests()
    {
        Checkpoint = 0;
        LastEvent = null;
        new ESDataGenerator().WriteTestEventsToStore(2).Wait();
    }

        Task EventAppeared(object ev, ulong checkpoint)
        {
            throw new ApplicationException("Bug in client code!");
        }

    [Fact]
    public async Task Should_fail_after_max_resubscriptions()
    {
        Subscription = new ESSubscription(new NullLoggerFactory().CreateLogger<ESSubscription>(), CreateEventStoreClient())
        {
            StreamName = TestProjection.StreamName,
            EventAppearedCallback = EventAppeared
        };
        await Subscription.StartAsync(0);
        await Task.Delay(500);

        Assert.True(Subscription.HasFailed);
        Assert.Equal("Bug in client code!", Subscription.Error);
    }

        static EventStoreClient CreateEventStoreClient()
        {
            var configuration = ConfigurationFactory.CreateConfiguration();
            var settings = EventStoreClientSettings.Create(configuration["EventStoreDB:ConnectionString"]);
            var cli = new EventStoreClient(settings);
            return cli;
        }
}
