using DStack.Aggregates.UnitTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Aggregates.Tests;

public class RegisterOrRenamePersonTests : PersonTester
{

    [Fact]
    public async Task Should_Register_Person()
    {
        var id = $"Persons-{Guid.NewGuid()}";
        var ev = new PersonRegisteredOrRenamed() { Id = id, Name = "Joseph" };
        var expectedProducedEvents = ToEventList(ev);
        var expectedPublishedEvents = expectedProducedEvents;

        Given();
        When(new RegisterOrRenamePerson() { Id = id, Name = "Joseph" });

        await Expect(expectedProducedEvents, expectedPublishedEvents);
    }

    [Fact]
    public async Task Should_Register_Or_Rename_Person()
    {
        var id = $"Persons-{Guid.NewGuid()}";
        var ev = new PersonRegisteredOrRenamed() { Id = id, Name = "Joseph" };
        
        Given(new PersonRegisteredOrRenamed() { Id = id, Name = "Shamso69" });
        When(new RegisterOrRenamePerson() { Id = id, Name = "Joseph" });
        
        var expectedProducedEvents = ToEventList(ev);
        var expectedPublishedEvents = expectedProducedEvents;

        await Expect(expectedProducedEvents, expectedPublishedEvents);
    }
}
