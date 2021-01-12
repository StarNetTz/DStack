using System;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Aggregates.UnitTests
{
    public class RegisterPersonTests : PersonTester
    {
        [Fact]
        public async Task Should_RegisterPerson()
        {
            var id = $"Persons-{Guid.NewGuid()}";
            var ev = new PersonRegistered() { Id = id, Name = "John" };
            var expectedProducedEvents = ToEventList(ev);
            var expectedPublishedEvents = expectedProducedEvents;

            Given();

            When(new RegisterPerson() { Id = id, Name = "John" });

            await Expect(expectedProducedEvents, expectedPublishedEvents);
        }

        [Fact]
        public async Task Should_Be_Idempotent()
        {
            var id = $"Persons-{Guid.NewGuid()}";

            Given(new PersonRegistered() { Id = id, Name = "John" });

            When(new RegisterPerson() { Id = id, Name = "John" });

            await ExpectNoEvents();
        }

        [Fact]
        public async Task Should_Throw_On_Non_Idempotent_Registration()
        {
            var id = $"Persons-{Guid.NewGuid()}";

            Given(new PersonRegistered() { Id = id, Name = "John" });
            When(new RegisterPerson() { Id = id, Name = "Danny" });
            await ExpectError("PersonAlreadyRegistered");
        }
    }
}