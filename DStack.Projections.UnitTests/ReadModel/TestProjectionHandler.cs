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

        public async Task Handle(dynamic @event, ulong checkpoint)
        {
            await When(@event, checkpoint);
        }

        public async Task When(TestEvent e, ulong checkpoint)
        {
            var doc = new TestModel { Id = e.Id, SomeValue = e.SomeValue };
            var time = TimeProvider.GetUtcNow();
            await Store.StoreAsync(doc);
        } 
    }

    public class TestProjectionWithCustomStoreHandler : IHandler
    {
        readonly ICustomStore Store;
        readonly ITimeProvider TimeProvider;

        public TestProjectionWithCustomStoreHandler(ICustomStore store, ITimeProvider timeProvider)
        {
            Store = store;
            TimeProvider = timeProvider;
        }

        public async Task Handle(dynamic @event, ulong checkpoint)
        {
            await When(@event, checkpoint);
        }

        public async Task When(TestEvent e, ulong checkpoint)
        {
            var doc = new TestModel { Id = e.Id, SomeValue = e.SomeValue };
            var time = TimeProvider.GetUtcNow();
            await Store.StoreAsync(doc);
        }
    }
}