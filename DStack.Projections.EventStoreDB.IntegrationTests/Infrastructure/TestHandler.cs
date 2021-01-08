using System.Threading.Tasks;

namespace DStack.Projections.ES.IntegrationTests
{
    public class TestHandler : IHandler
    {
        public async Task Handle(dynamic @event, long checkpoint)
        {
            await When(@event, checkpoint);
        }

        public Task When(TestEvent e, long checkpoint)
        {
            return Task.CompletedTask;
        }
    }
}
