﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DStack.Aggregates;

public abstract class Interactor<TAggregate> : IInteractor
    where TAggregate : class, IAggregate, new()
{
    protected List<object> PublishedEvents;

    protected IAggregateRepository AggregateRepository;

    private static string NotFoundMessage
    {
        get
        {
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

    protected virtual async Task IdempotentlyCreateAgg(string id, Action<TAggregate> usingThisMethod)
    {
        var agg = await AggregateRepository.GetAsync<TAggregate>(id);

        if (agg == null)
            agg = new TAggregate();

        var ov = agg.Version;

        usingThisMethod(agg);
        PublishedEvents = agg.PublishedEvents;

        if (ov != agg.Version)
            await AggregateRepository.StoreAsync(agg);
    }

    protected virtual async Task IdempotentlyCreateAggAsync(string id, Func<TAggregate, Task> usingThisMethod)
    {
        var agg = await AggregateRepository.GetAsync<TAggregate>(id);

        if (agg == null)
            agg = new TAggregate();

        var ov = agg.Version;

        await usingThisMethod(agg);
        PublishedEvents = agg.PublishedEvents;

        if (ov != agg.Version)
            await AggregateRepository.StoreAsync(agg);
    }

    protected virtual async Task IdempotentlyUpdateAgg(string id, Action<TAggregate> usingThisMethod)
    {
        var agg = await AggregateRepository.GetAsync<TAggregate>(id);

        if (agg == null)
            throw DomainError.Named(NotFoundMessage, string.Empty);

        var ov = agg.Version;

        usingThisMethod(agg);
        PublishedEvents = agg.PublishedEvents;

        if (ov != agg.Version)
            await AggregateRepository.StoreAsync(agg);
    }

    protected virtual async Task IdempotentlyUpdateAggAsync(string id, Func<TAggregate, Task> usingThisMethod)
    {
        var agg = await AggregateRepository.GetAsync<TAggregate>(id);

        if (agg == null)
            throw DomainError.Named(NotFoundMessage, string.Empty);

        var ov = agg.Version;

        await usingThisMethod(agg);
        PublishedEvents = agg.PublishedEvents;

        if (ov != agg.Version)
            await AggregateRepository.StoreAsync(agg);
    }

    protected virtual Task When(object ev)
    {
        throw new NotImplementedOnInteractorException(
            $"Interactor handler for command: 'When({ev.GetType().Name} c)' is not implemented inside: '{GetType().Name}'."
        );
    }
}