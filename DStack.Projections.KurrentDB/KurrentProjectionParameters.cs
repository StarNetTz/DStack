using System;
using System.Collections.Generic;

namespace DStack.Projections.KurrentDB;

public class KurrentProjectionParameters
{
    public string Name { get; set; }
    public List<string> SourceStreamNames { get; set; }
    public string DestinationStreamName { get; set; }
    public Type[] EventsToInclude { get; set; }
}
