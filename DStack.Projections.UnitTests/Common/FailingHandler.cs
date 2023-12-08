using System;
using System.Threading.Tasks;

namespace DStack.Projections.Tests;

public class FailingHandler : IHandler
{
    public async Task Handle(dynamic @event, ulong checkpoint) { await When(@event, checkpoint); }

    public Task When(TestEvent e, ulong checkpoint)
    {
        throw new ApplicationException("I Failed bro!");
    }
}