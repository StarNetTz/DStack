using System.Collections.Generic;

namespace DStack.Aggregates;

public abstract class Aggregate : IAggregate
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