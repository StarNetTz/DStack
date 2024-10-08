namespace DStack.Aggregates;

public class PersonAggregateState : AggregateState
{

    internal string Name { get; set; }

    protected override void DelegateWhenToConcreteClass(object ev)
    {
        When((dynamic)ev);
    }

    void When(PersonRegistered e)
    {
        Id = e.Id;
        Name = e.Name;
    }

    void When(PersonRenamed e)
    {
        Name = e.Name;
    }

    void When(PersonRegisteredOrRenamed e)
    {
        Id = e.Id;
        Name = e.Name;
    }
}