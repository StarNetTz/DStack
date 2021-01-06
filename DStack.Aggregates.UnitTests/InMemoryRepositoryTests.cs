using System;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Aggregates.Facts
{
    public class InMemoryRepositoryTests
    {
        InMemoryAggregateRepository Repository;

        public InMemoryRepositoryTests()
        {
            Repository = new InMemoryAggregateRepository();
        }

        [Fact]
        public async Task Get_With_An_Empty_Id_Should_Return_Null()
        {
            var agg = await Repository.GetAsync<PersonAggregate>("");
            Assert.Null(agg);
        }
        
        [Fact]
        public async Task Should_Store_And_Get()
        {
            var id = $"Persons-{Guid.NewGuid()}";
            await Repository.StoreAsync(PersonAggregateFactory.Create(id, "Joe"));
            var pl = await Repository.GetAsync<PersonAggregate>(id);
            Assert.Equal(1, pl.Version);
        }

        [Fact]
        public async Task Store_Should_Clear_Changes()
        {
            var id = $"Persons-{Guid.NewGuid()}";
            var p = PersonAggregateFactory.Create(id, "Mary");
            await Repository.StoreAsync(p);
            Assert.Empty(p.Changes);
        }

        [Fact]
        public async Task Should_Get_Any_Version_Of_An_Aggregate()
        {
            var id = $"Persons-{Guid.NewGuid()}";
            int finalVersion = 5;
            int targetVersion = 3;
            await CreateAndStoreUpdatedAggregate(id, finalVersion);
            var agg = await Repository.GetAsync<PersonAggregate>(id, targetVersion);
            Assert.Equal(3, agg.Version);
        }

            async Task CreateAndStoreUpdatedAggregate(string aggId, int nrOfUpdates)
            {
                var p = PersonAggregateFactory.CreateWithUncommitedUpdates(aggId, nrOfUpdates);
                await Repository.StoreAsync(p);
            }

        [Fact]
        public async Task Should_Throw_On_Concurrent_Update()
        {
           var id = $"Persons-{Guid.NewGuid()}";
           await Repository.StoreAsync(PersonAggregateFactory.Create(id, "Joe"));
           var p = await Repository.GetAsync<PersonAggregate>(id);

           await UpdateOutOfSession(id, Repository);

           UpdateInSession(p);

           await Assert.ThrowsAsync<ConcurrencyException>(async () => await Repository.StoreAsync(p));
        }

            async Task UpdateOutOfSession(string aggId, IAggregateRepository rep)
            {
                var agg = await rep.GetAsync<PersonAggregate>(aggId);
                agg.RenameForIntegrationTestingPurposes(new RenamePerson() { Id = aggId, Name = "Joey" });
                await rep.StoreAsync(agg);
            }

            void UpdateInSession(PersonAggregate agg)
            {
                agg.RenameForIntegrationTestingPurposes(new RenamePerson { Id = agg.Id, Name = "Jake" });
            }
    }
}