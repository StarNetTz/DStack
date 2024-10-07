using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DStack.Aggregates;

public abstract class Aggregate<TAggregateState> : IAggregate
    where TAggregateState : AggregateState, new()
{
    IAggregateState State;

    public List<object> Changes { get; private set; }
    public List<object> PublishedEvents { get; private set; }

    public string Id
    {
        get
        {
            return State.Id;
        }
    }

    public int Version
    {
        get
        {
            return State.Version;
        }
    }

    public Aggregate()
    {
        State = new TAggregateState();
        Changes = new List<object>();
        PublishedEvents = new List<object>();
    }

    public Aggregate(IAggregateState state)
    {
        State = state;
        Changes = new List<object>();
        PublishedEvents = new List<object>();
    }

    protected void Apply(object @event)
    {
        State.Mutate(@event);
        Changes.Add(@event);
    }
}