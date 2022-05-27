using Raven.Client.Documents;
using System.Threading.Tasks;

namespace DStack.Projections.RavenDB
{
    public class RavenDBCheckpointReader : ICheckpointReader
    {
        readonly IDocumentStore DocumentStore;

        public RavenDBCheckpointReader(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

        public async Task<Checkpoint> Read(string id)
        {
            using (var s = DocumentStore.OpenAsyncSession())
            {
                var chp = await s.LoadAsync<Checkpoint>(id).ConfigureAwait(false);
                if (null == chp)
                    chp = new Checkpoint { Id = id, Value = 0 };
                return chp;
            }
        }
    }
}