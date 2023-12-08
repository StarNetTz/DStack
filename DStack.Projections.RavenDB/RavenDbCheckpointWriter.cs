using Raven.Client.Documents;
using System.Threading.Tasks;

namespace DStack.Projections.RavenDB;

public class RavenDBCheckpointWriter : ICheckpointWriter
{
    readonly IDocumentStore DocumentStore;

    public RavenDBCheckpointWriter(IDocumentStore documentStore)
    {
        DocumentStore = documentStore;
    }

    public async Task Write(Checkpoint checkpoint)
    {
        using (var s = DocumentStore.OpenAsyncSession())
        {
            await s.StoreAsync(checkpoint).ConfigureAwait(false);
            await s.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}