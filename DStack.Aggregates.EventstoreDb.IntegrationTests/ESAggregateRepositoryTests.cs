using EventStore.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Aggregates.EventStoreDB.IntegrationFacts;

public class ESAggregateRepositoryTests : IClassFixture<DatabaseFixture>
{
    DatabaseFixture Fixture;

    public ESAggregateRepositoryTests(DatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    [Fact]
    public async Task Should_Store_And_Load()
    {
        var id = $"Persons-{Guid.NewGuid()}";
        await Fixture.Repository.StoreAsync(PersonAggregateFactory.Create(id, "Joe"));
        var agg = await Fixture.Repository.GetAsync<PersonAggregate>(id);

        Assert.Equal(1, agg.Version);
    }

    [Fact]
    public async Task Store_Should_Reset_List_Of_Changes()
    {
        var agg = PersonAggregateFactory.Create($"Persons-{Guid.NewGuid()}", "Joe");
        await Fixture.Repository.StoreAsync(agg);
        Assert.Empty(agg.Changes);
    }

    [Fact]
    public async Task Should_Get_Specified_Version_Of_The_Aggregate()
    {
        const int NrOfEventsToAdd = 5;
        const int FinalVersion = 6;
        const int RequestedVersion = 2;

        var id = $"Persons-{Guid.NewGuid()}";

        var agg = PersonAggregateFactory.CreateWithUncommitedUpdates(id, NrOfEventsToAdd);
        await Fixture.Repository.StoreAsync(agg);

        var loadedAggregate = await Fixture.Repository.GetAsync<PersonAggregate>(id, RequestedVersion);

        Assert.Equal(FinalVersion, agg.Version);
        Assert.Equal(RequestedVersion, loadedAggregate.Version);
    }

    [Fact]
    public async Task Get_Should_Return_Null_If_Id_Was_Not_Found()
    {
        var id = $"Persons-{Guid.NewGuid()}";
        var agg = await Fixture.Repository.GetAsync<PersonAggregate>(id);
        Assert.Null(agg);
    }

    [Fact]
    public async Task Concurrent_Updates_Should_Throw_ConcurrencyException()
    {
        var id = $"Persons-{Guid.NewGuid()}";
        await Fixture.Repository.StoreAsync(PersonAggregateFactory.Create(id, "Joe"));
        var agg = await Fixture.Repository.GetAsync<PersonAggregate>(id);
        await UpdateOutOfSession(id, Fixture.Repository);
        UpdateInSession(agg);
        await Assert.ThrowsAsync<ConcurrencyException>(async () => { await Fixture.Repository.StoreAsync(agg); });
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

public class DatabaseFixture
{
    public ESAggregateRepository Repository;

    public DatabaseFixture()
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, false).Build();
        var settings = EventStoreClientSettings.Create(configuration["EventStoreDB:ConnectionString"]);
        var client = new EventStoreClient(settings);
        Repository = new ESAggregateRepository(client);
    }
}