namespace DStack.Aggregates;

public class PersonAggregate : Aggregate<PersonAggregateState>
{
    public PersonAggregate() : base() { }

    internal void Create(RegisterPerson cmd)
    {
        if (ShouldHandleIdempotency)
            if (cmd.IsIdempotent(State))
                return;
            else
                throw DomainError.Named("PersonAlreadyRegistered", $"A person named {State.Name} is already registered with id {State.Id}");
        var e = cmd.ToEvent();
        Apply(e);
        PublishedEvents.Add(e);
    }

    internal void Rename(RenamePerson cmd)
    {
        if (ShouldHandleIdempotency)
            if (cmd.IsIdempotent(State))
                return;
        Apply(cmd.ToEvent());
    }

    public void RenameForIntegrationTestingPurposes(RenamePerson cmd)
    {
        Rename(cmd);
    }
}
