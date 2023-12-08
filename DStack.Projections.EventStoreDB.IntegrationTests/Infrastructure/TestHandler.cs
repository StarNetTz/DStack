using System.Threading.Tasks;

namespace DStack.Projections.EventStoreDB.IntegrationTests;

public class TestHandler : IHandler
{
    public async Task Handle(dynamic @event, ulong checkpoint)
    {
        await When(@event, checkpoint);
    }

    public Task When(TestEvent e, ulong checkpoint)
    {
        return Task.CompletedTask;
    }
}
