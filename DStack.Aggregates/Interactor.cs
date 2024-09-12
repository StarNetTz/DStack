using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DStack.Aggregates;

public abstract class Interactor<TAggregate, TAggregateState> : IInteractor
    where TAggregate : class, IAggregate
    where TAggregateState : class, IAggregateState, new()
{
    protected List<object> PublishedEvents;

    protected IAggregateRepository AggregateRepository;

    private string NotFoundMessage
    {
        get {
            string AggregateName = typeof(TAggregate).Name.Replace("Aggregate", "", StringComparison.InvariantCultureIgnoreCase);

            return $"{AggregateName}DoesNotExist";
        }
    }

    public abstract Task ExecuteAsync(object command);

    public Interactor()
    {
        PublishedEvents = new List<object>();
    }

    public List<object> GetPublishedEvents()
    {
        return PublishedEvents;
    }

    protected async Task IdempotentlyCreateAgg(string id, Action<TAggregate> usingThisMethod)
    {
        var agg = await AggregateRepository.GetAsync<TAggregate>(id);

        if (agg == null)
        {
            agg = (TAggregate)Activator.CreateInstance(typeof(TAggregate), [new TAggregateState()]);
        }

        var ov = agg.Version;

        usingThisMethod(agg);
        PublishedEvents = agg.PublishedEvents;

        if (ov != agg.Version)
            await AggregateRepository.StoreAsync(agg);
    }

    protected async Task IdempotentlyUpdateAgg(string id, Action<TAggregate> usingThisMethod)
    {
        var agg = await AggregateRepository.GetAsync<TAggregate>(id);

        if (agg == null)
        {
            throw DomainError.Named(NotFoundMessage, string.Empty);
        }

        var ov = agg.Version;

        usingThisMethod(agg);
        PublishedEvents = agg.PublishedEvents;

        if (ov != agg.Version)
            await AggregateRepository.StoreAsync(agg);
    }
}
