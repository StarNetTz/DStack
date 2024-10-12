﻿namespace DStack.Aggregates;


public static class PersonCommandExtensions
{
    public static bool IsIdempotent(this RegisterPerson cmd, PersonAggregateState state)
    {
        return cmd.Name == state.Name;
    }

    public static PersonRegistered ToEvent(this RegisterPerson cmd)
    {
        return new PersonRegistered()
        {
            Id = cmd.Id,
            Name = cmd.Name
        };
    }

    public static bool IsIdempotent(this RenamePerson cmd, PersonAggregateState state)
    {
        return cmd.Name == state.Name;
    }

    public static PersonRenamed ToEvent(this RenamePerson cmd)
    {
        return new PersonRenamed()
        {
            Id = cmd.Id,
            Name = cmd.Name
        };
    }
}