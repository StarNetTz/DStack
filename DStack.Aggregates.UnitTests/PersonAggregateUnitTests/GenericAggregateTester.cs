using DStack.Aggregates.Testing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DStack.Aggregates.UnitTests;


/// <summary>
/// Aggregate tester that encapsulates most common testing scenario
/// </summary>
/// <typeparam name="TInteractor"></typeparam>
public class AggregateTester<TInteractor> : AggregateTesterBase<ICommand, IEvent> where TInteractor : IInteractor
{
    protected TInteractor Tester { get; set; }
    protected BDDAggregateRepository Repository { get; set; }


    //A hook to create interactor with custom dependencies
    public virtual void Initialize()
    {

        Tester = (TInteractor)Activator.CreateInstance(typeof(TInteractor), Repository);
    }

    protected override async Task<ExecuteCommandResult<IEvent>> ExecuteCommand(IEvent[] given, ICommand cmd)
    {
        Repository = new BDDAggregateRepository();
        Repository.Preload(cmd.Id, given);
        Initialize();
        await Tester.ExecuteAsync(cmd);
        var publishedEvents = Tester.GetPublishedEvents();
        var arr = Repository.Appended != null ? Repository.Appended.Cast<IEvent>().ToArray() : null;
        var res = new ExecuteCommandResult<IEvent>
        {
            ProducedEvents = arr ?? new IEvent[0],
            PublishedEvents = publishedEvents.Cast<IEvent>().ToArray()
        };
        return res;
    }
}