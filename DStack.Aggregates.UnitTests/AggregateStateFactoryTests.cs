using Xunit;

namespace DStack.Aggregates.Tests
{
    public class AggregateStateFactoryTests
    {
        [Fact]
        public void Should_Create_PersonAggregateState()
        {
            var state = AggregateStateFactory.CreateStateFor(typeof(PersonAggregate));
            Assert.IsType<PersonAggregateState>(state);
        }
    }
}