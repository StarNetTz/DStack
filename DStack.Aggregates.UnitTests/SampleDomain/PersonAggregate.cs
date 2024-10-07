namespace DStack.Aggregates;

public class PersonAggregate : Aggregate<PersonAggregateState>
{
    public PersonAggregate() : base() { }
    public PersonAggregate(PersonAggregateState state) { State = state; }

    internal void Create(RegisterPerson cmd)
    {
        if (State.Version > 0)
            if (State.Name == cmd.Name)
                return;
            else
                throw DomainError.Named("PersonAlreadyRegistered", $"A person named {State.Name} is already registered with id {State.Id}");
        var e = new PersonRegistered() { Id = cmd.Id, Name = cmd.Name };
        Apply(e);
        PublishedEvents.Add(e);
    }

    internal void Rename(RenamePerson cmd)
    {
        if (State.Name == cmd.Name)
            return;
        Apply(new PersonRenamed() { Id = cmd.Id, Name = cmd.Name });
    }

    public void RenameForIntegrationTestingPurposes (RenamePerson cmd)
    {
        Rename(cmd);
    }
}
