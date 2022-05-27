using DStack.Aggregates.Testing;
using System.Linq;
using System.Threading.Tasks;

namespace DStack.Aggregates.UnitTests
{
    public class PersonTester : AggregateTesterBase<ICommand, IEvent>
    {
        protected override async Task<ExecuteCommandResult<IEvent>> ExecuteCommand(IEvent[] given, ICommand cmd)
        {
            var repository = new BDDAggregateRepository();
            repository.Preload(cmd.Id, given);
            var tester = new PersonAggregateInteractor(repository); //inject interactor specific dependencies here
            await tester.ExecuteAsync(cmd);
            var publishedEvents = tester.GetPublishedEvents();
            var arr = (repository.Appended != null) ? repository.Appended.Cast<IEvent>().ToArray() : null;
            var res = new ExecuteCommandResult<IEvent> {
                ProducedEvents = arr ?? new IEvent[0], PublishedEvents = publishedEvents.Cast<IEvent>().ToArray()
            };
            return res;
        }
    }
}