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

    async Task When(RegisterPersonWithAsync c)
    {
        await IdempotentlyCreateAggAsync(c.Id, async agg => await agg.CreateAsync(c));
    }


    async Task When(RenamePerson c)
    {
        await IdempotentlyUpdateAgg(c.Id, agg => agg.Rename(c));
    }

    async Task When(RenamePersonWithAsync c)
    {
        await IdempotentlyUpdateAggAsync(c.Id, async agg => await agg.RenameAsync(c));
    }
}
