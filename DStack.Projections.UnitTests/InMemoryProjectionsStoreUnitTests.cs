using DStack.Projections.Testing;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.UnitTests;

public class InMemoryProjectionsStoreUnitTests
{
    [Fact]
    public async Task Should_Return_Null_If_Document_Is_Not_Found()
    {
        var store = new InMemoryProjectionsStore();
        var doc = await store.LoadAsync<DocumentWithNumericId>("1");

        Assert.Null(doc);
    }

    [Fact]
    public async Task Should_Store_And_Load_Typed_Document()
    {
        var store = new InMemoryProjectionsStore();
        await store.StoreAsync(new DocumentWithNumericId { Id = 1 });

        var doc = await store.LoadAsync<DocumentWithNumericId>("1");

        Assert.Equal(1, doc.Id);
    }

    [Fact]
    public async Task Should_Store_Object()
    {
        var store = new InMemoryProjectionsStore();
        object doc = new DocumentWithNumericId { Id = 1 };
        await store.StoreAsync(doc);

        var loadedDoc = await store.LoadAsync<DocumentWithNumericId>("1");

        Assert.Equal(1, loadedDoc.Id);
    }


    [Fact]
    public async Task Should_Delete()
    {
        var store = new InMemoryProjectionsStore();
        object doc = new DocumentWithNumericId { Id = 1 };
        await store.StoreAsync(doc);
        await store.DeleteAsync("1");

        var loadedDoc = await store.LoadAsync<DocumentWithNumericId>("1");

        Assert.Null(loadedDoc);
    }

    [Fact]
    public async Task Should_DeleteInUnitOfWorkAsync()
    {
        var store = new InMemoryProjectionsStore();
        object doc = new DocumentWithNumericId { Id = 1 };
        await store.StoreAsync(doc);
        await store.DeleteInUnitOfWorkAsync("1");
        var loadedDoc = await store.LoadAsync<DocumentWithNumericId>("1");
        Assert.Null(loadedDoc);
    }

    [Fact]
    public async Task Should_Store_Typed_Documents_In_UnitOfWork()
    {
        var store = new InMemoryProjectionsStore();
        var doc1 = new DocumentWithNumericId { Id = 1 };
        var doc2 = new DocumentWithNumericId { Id = 2 };
        await store.StoreInUnitOfWorkAsync(doc1, doc2);

        var loadedDoc = await store.LoadAsync<DocumentWithNumericId>("1");

        Assert.Equal(1, loadedDoc.Id);
    }

    [Fact]
    public async Task Should_Store_Objects_In_UnitOfWork()
    {
        var store = new InMemoryProjectionsStore();
        object doc1 = new DocumentWithNumericId { Id = 1 };
        object doc2 = new DocumentWithNumericId { Id = 2 };
        await store.StoreInUnitOfWorkAsync(doc1, doc2);

        var loadedDoc = await store.LoadAsync<DocumentWithNumericId>("1");

        Assert.Equal(1, loadedDoc.Id);
    }

    [Fact]
    public async Task Should_Load_Multiple_Documents_By_Ids()
    {
        var store = new InMemoryProjectionsStore();
        var doc1 = new DocumentWithNumericId { Id = 1 };
        var doc2 = new DocumentWithNumericId { Id = 2 };
        await store.StoreInUnitOfWorkAsync(doc1, doc2);

        var docs = await store.LoadAsync<DocumentWithNumericId>("1", "2");

        Assert.Equal(1, docs["1"].Id);
        Assert.Equal(2, docs["2"].Id);
    }

    [Fact]
    public async Task Throws_On_Unsupporeted_Id()
    {
        var s = new InMemoryProjectionsStore();
        var d = new DocumentWithUnsupportedId { Id = 1 };

        await Assert.ThrowsAsync<ArgumentException>(async () => { await s.StoreAsync(d); });
    }
}

public class DocumentWithNumericId
{
    public long Id { get; set; }
}

public class DocumentWithUnsupportedId
{
    public short Id { get; set; }
}