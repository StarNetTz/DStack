using DStack.Aggregates.UnitTests;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Aggregates.Tests;

public class RenamePersonTests : AggregateTester<PersonAggregateInteractor>
{
    [Fact]
    public async Task Should_Rename()
    {
        var id = $"Persons-{Guid.NewGuid()}";

        Given(new PersonRegistered() { Id = id, Name = "John" });

        When(new RenamePerson() { Id = id, Name = "Gary" });

        await Expect(new PersonRenamed() { Id = id, Name = "Gary" });
    }

    [Fact]
    public async Task Rename_Is_Idempotent()
    {
        var id = $"Persons-{Guid.NewGuid()}";

        Given(new PersonRegistered() { Id = id, Name = "John" });

        When(new RenamePerson() { Id = id, Name = "John" });

        await ExpectNoEvents();
    }

    [Fact]
    public async Task Should_Throw_On_NonExistant_Person()
    {
        var id = $"Persons-{Guid.NewGuid()}";

        Given();
        When(new RenamePerson() { Id = id, Name = "James" });

        await ExpectError("PersonDoesNotExist");
    }

    [Fact]
    public async Task Should_RenameWithAsync()
    {
        var id = $"Persons-{Guid.NewGuid()}";

        Given(new PersonRegisteredWithAsync() { Id = id, Name = "John" });

        When(new RenamePersonWithAsync() { Id = id, Name = "Gary" });

        await Expect(new PersonRenamedWithAsync() { Id = id, Name = "Gary" });
    }

    [Fact]
    public async Task Should_Throw_On_NonExistant_Person_WithAsync()
    {
        var id = $"Persons-{Guid.NewGuid()}";

        Given();
        When(new RenamePersonWithAsync() { Id = id, Name = "James" });

        await ExpectError("PersonDoesNotExist");
    }
}
