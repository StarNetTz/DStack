﻿using System.Collections.Generic;

namespace DStack.Aggregates;

public abstract class Aggregate<TAggregateState> : IAggregate
    where TAggregateState : AggregateState, new()
{
    public TAggregateState State;

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

    public bool ShouldHandleIdempotency => State.Version > 0;

    public Aggregate()
    {
        State = new TAggregateState();
        Changes = new List<object>();
        PublishedEvents = new List<object>();
    }

    public void SetState(dynamic state)
    {
        State = state;
    }

    protected void Apply(object @event)
    {
        State.Mutate(@event);
        Changes.Add(@event);
    }
}