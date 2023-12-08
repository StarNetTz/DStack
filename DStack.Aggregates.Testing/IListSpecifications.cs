using System.Collections.Generic;

namespace DStack.Aggregates.Testing;

public interface List<TCommand, TEvent>
{
    IEnumerable<SpecificationInfo<TCommand, TEvent>> ListSpecifications();
}
