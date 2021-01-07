using DStack.Projections.UnitTests;
using System.Threading.Tasks;

namespace DStack.Projections.Tests
{
    public class TestProjectionHandler : IHandler
    {
        readonly INoSqlStore Store;
        readonly ITimeProvider TimeProvider;

        public TestProjectionHandler(INoSqlStore store, ITimeProvider timeProvider)
        {
            Store = store;
            TimeProvider = timeProvider;
        }

        public async Task Handle(dynamic @event, long checkpoint)
        {
            await When(@event, checkpoint);
        }

        public async Task When(TestEvent e, long checkpoint)
        {
            var doc = new TestModel { Id = e.Id, SomeValue = e.SomeValue };
            var time = TimeProvider.GetUtcNow();
            await Store.StoreAsync(doc);
        } 
    }
}