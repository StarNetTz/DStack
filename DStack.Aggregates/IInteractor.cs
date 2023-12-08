using System.Collections.Generic;
using System.Threading.Tasks;

namespace DStack.Aggregates;

public interface IInteractor
{
    Task ExecuteAsync(object command);
    List<object> GetPublishedEvents();
}

public abstract class Interactor : IInteractor
{
    protected List<object> PublishedEvents;

    public abstract Task ExecuteAsync(object command);

    public Interactor()
    {
        PublishedEvents = new List<object>();
    }

    public List<object> GetPublishedEvents()
    {
        return PublishedEvents;
    }
}