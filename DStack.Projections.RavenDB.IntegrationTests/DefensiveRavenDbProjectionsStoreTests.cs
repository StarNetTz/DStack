using System;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.RavenDB.IntegrationTests;

public class DefensiveRavenDBProjectionsStoreTests : IntegrationTestBase
{
    [Fact]
    public async Task ShouldSaveAndLoadDocument()
    {
        var repo = new DefensiveRavenDBProjectionsStore(DocumentStore);
        var id = $"TestDocuments/{Guid.NewGuid()}";
        
        await repo.StoreAsync(new TestDocument { Id = id, SomeProp = "Hello1" });
        var doc = await repo.LoadAsync<TestDocument>(id);

        Assert.Equal(id, doc.Id);
        Assert.Equal("Hello1", doc.SomeProp);
    }

    [Fact]
    public async Task ShouldSaveAndLoadDocuments()
    {
        var repo = new DefensiveRavenDBProjectionsStore(DocumentStore);
        var id1 = $"TestDocuments/{Guid.NewGuid()}";
        var id2 = $"TestDocuments/{Guid.NewGuid()}";

        var doc1 = new TestDocument { Id = id1, SomeProp = "Hello1" };
        var doc2 = new TestDocument { Id = id2, SomeProp = "Hello2" };

        await repo.StoreInUnitOfWorkAsync(doc1, doc2);
        var docs = await repo.LoadAsync<TestDocument>(id1, id2);

        Assert.Equal("Hello1", docs[id1].SomeProp);
        Assert.Equal("Hello2", docs[id2].SomeProp);
    }
}

public class TestDocument
{
    public string Id { get; set; }
    public string SomeProp { get; set; }
}