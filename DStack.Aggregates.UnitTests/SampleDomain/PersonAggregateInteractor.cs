using System;
using System.Threading.Tasks;

namespace DStack.Aggregates;

public interface IPersonAggregateInteractor : IInteractor { }

public class PersonAggregateInteractor : Interactor<PersonAggregate>, IPersonAggregateInteractor
{
    public PersonAggregateInteractor(IAggregateRepository aggRepository)
    {
        AggregateRepository = aggRepository;
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

    async Task When(RegisterOrRenamePerson c)
    {
        await IdempotentlyCreateOrUpdateAgg(c.Id, agg => agg.CreateOrRename(c));
    }
}
