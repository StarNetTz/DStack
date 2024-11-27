using DStack.Aggregates.UnitTests;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Aggregates.Tests;

public class NotImplementedTests : AggregateTester<PersonAggregateInteractor>
{
    [Fact]
    public async Task Should_RaiseException_When_InteractorState_IsNotImplemented()
    {
        var id = $"Persons-{Guid.NewGuid()}";

        Given(new PersonNotImplemented() { Id = id });
        When(new PersonDummyNotImplemented() { Id = id });

        await Assert.ThrowsAsync<NotImplementedException>(async () => await Expect());
    }
}
