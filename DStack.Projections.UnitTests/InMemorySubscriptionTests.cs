using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.Tests
{
    public class InMemorySubscriptionTests
    {
        InMemorySubscription Subscription;

        long Checkpoint = 0;
        object LastEvent = null;

        private Task EventAppeared(object ev, long checkpoint)
        {
            Checkpoint = checkpoint;
            LastEvent = ev;
            return Task.CompletedTask;
        }

        public InMemorySubscriptionTests()
        {
            LastEvent = null;
            Subscription = new InMemorySubscription() { StreamName = "myStream", EventAppearedCallback = EventAppeared };
        }


        [Fact]
        public async Task can_project()
        {
            Subscription.LoadEvents(new TestEvent() { Id = "1", SomeValue = "Manchester - Sloboda" });
            await Subscription.Start(0);
            AssertThatEventProjectedAsExpected();
        }

            void AssertThatEventProjectedAsExpected()
            {
                var e = LastEvent as TestEvent;
                Assert.Equal(1, Checkpoint);
                Assert.Equal("1", e.Id);
                Assert.Equal("Manchester - Sloboda", e.SomeValue);
            }

        [Fact]
        public async Task can_project_multiple_events()
        {
            LoadTwoEvents();
            await Subscription.Start(0);
            AssertLastEvent();
        }

        [Fact]
        public async Task can_read_stream_from_given_checkpoint_of_2()
        {
            LoadTwoEvents();
            await Subscription.Start(2);
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
            Assert.Equal(2, Checkpoint);
            Assert.Equal("2", e.Id);
            Assert.Equal("Manchester - Sloboda City", e.SomeValue);
        }
    }
}