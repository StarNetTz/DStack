using System;
using System.Threading.Tasks;

namespace DStack.Aggregates
{
    public interface IPersonAggregateInteractor : IInteractor { }

    public class PersonAggregateInteractor : Interactor, IPersonAggregateInteractor
    {
        readonly IAggregateRepository AggRepository;

        public PersonAggregateInteractor(IAggregateRepository aggRepository)
        {
            AggRepository = aggRepository;
        }

        public override async Task ExecuteAsync(object command)
        {
            await When((dynamic)command);
        }

        async Task When(RegisterPerson c)
        {
            await IdempotentlyCreateAgg(c.Id, agg => agg.Create(c));
        }

        async Task When(RenamePerson c)
        {
            await IdempotentlyUpdateAgg(c.Id, agg => agg.Rename(c));
        }

        async Task IdempotentlyCreateAgg(string id, Action<PersonAggregate> usingThisMethod)
        {
            var agg = await AggRepository.GetAsync<PersonAggregate>(id).ConfigureAwait(false);
            if (agg == null)
                agg = new PersonAggregate(new PersonAggregateState());
            var ov = agg.Version;
            usingThisMethod(agg);
            PublishedEvents = agg.PublishedEvents;
            if (ov != agg.Version)
                await AggRepository.StoreAsync(agg).ConfigureAwait(false);
        }

        async Task IdempotentlyUpdateAgg(string id, Action<PersonAggregate> usingThisMethod)
        {
            var agg = await AggRepository.GetAsync<PersonAggregate>(id).ConfigureAwait(false);
            var ov = agg.Version;
            usingThisMethod(agg);
            PublishedEvents = agg.PublishedEvents;
            if (ov == agg.Version)
                return;
            await AggRepository.StoreAsync(agg).ConfigureAwait(false);
        }
    }
}
