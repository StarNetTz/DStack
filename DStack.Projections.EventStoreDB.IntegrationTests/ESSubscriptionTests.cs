using EventStore.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.EventStoreDB.IntegrationTests
{
    public class ESSubscriptionTests
    {
        ESSubscription Subscription;

        ulong Checkpoint = 0;
        object LastEvent = null;

        public ESSubscriptionTests()
        {
            Checkpoint = 0;
            LastEvent = null;
            new ESDataGenerator().WriteTestEventsToStore(2).Wait();
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
            Subscription = new ESSubscription(new NullLoggerFactory().CreateLogger<ESSubscription>(), CreateEventStoreClient())
            {
                StreamName = TestProjection.StreamName,
                EventAppearedCallback = EventAppeared
            };

            await Subscription.StartAsync(0);
            await Task.Delay(200);

            AssertThatEventsProjected();
        }

            static EventStoreClient CreateEventStoreClient()
            {
                var configuration = ConfigurationFactory.CreateConfiguration();
                var settings = EventStoreClientSettings.Create(configuration["EventStoreDB:ConnectionString"]);
                var cli = new EventStoreClient(settings);
                return cli;
            }

            void AssertThatEventsProjected()
            {
                Assert.IsType<TestEvent>(LastEvent);
                Assert.NotNull(LastEvent);
                Assert.Contains("A guid:", (LastEvent as TestEvent).SomeValue);
                Assert.True(Checkpoint > 0);
            }
    }
}
