using DStack.Aggregates.UnitTests;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Aggregates.Tests;

public class NotImplementedTests : AggregateTester<PersonAggregateInteractor>
{
    [Fact]
    public async Task Should_RaiseException_When_InteractorHandler_IsNotImplemented()
    {
        var id = $"Persons-{Guid.NewGuid()}";

        Given(new PersonRenamed() { Id = id });
        When(new CommandNotImplementedOnInteractor() { Id = id });

        await Assert.ThrowsAsync<NotImplementedOnInteractorException>(async () => await Expect());
    }

    [Fact]
    public async Task Should_RaiseException_When_AggregateStateHandler_IsNotImplemented()
    {
        var id = $"Persons-{Guid.NewGuid()}";

        Given(new EventNotImplementedOnAggregateState() { Id = id });
        When(new RenamePersonWithAsync() { Id = id, Name = "Francis Walsingham" });

        await Assert.ThrowsAsync<NotImplementedOnAggregateStateException>(async () => await Expect());
    }
}
