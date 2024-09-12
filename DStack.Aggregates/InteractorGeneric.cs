using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DStack.Aggregates;

public abstract class InteractorGeneric
{
    protected List<object> PublishedEvents;

    public abstract Task ExecuteAsync(object command);

    public InteractorGeneric()
    {
        PublishedEvents = new List<object>();
    }

    public List<object> GetPublishedEvents()
    {
        return PublishedEvents;
    }
}
