using System.Collections.Generic;

namespace DStack.Aggregates;

public interface IAggregate
{
    string Id { get; }

    int Version { get; }

    List<object> Changes { get; }
    List<object> PublishedEvents { get; }
}