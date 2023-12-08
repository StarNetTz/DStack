namespace DStack.Aggregates;

public class PersonAggregateFactory
{
    public static PersonAggregate Create(string id, string name)
    {
        var agg = new PersonAggregate(new PersonAggregateState());
        agg.Create(new RegisterPerson() { Id = id, Name = name });
        return agg;
    }
    public static PersonAggregate CreateWithUncommitedUpdates(string id, int nrOfUpdates)
    {
        var p = new PersonAggregate(new PersonAggregateState());
        p.Create(new RegisterPerson() { Id = id, Name = "0" });
        for (int i = 0; i < nrOfUpdates; i++)
            p.Rename(new RenamePerson() { Id = id, Name = $"Name {i + 1}" });
        return p;
    }
}
