using Raven.Client.Documents;
using System;
using System.Threading.Tasks;

namespace DStack.Projections.RavenDB;

public class DefensiveRavenDBCheckpointReader : ICheckpointReader
{
    private int MaxRetries = 3;
    private readonly TimeSpan Delay = TimeSpan.FromMilliseconds(50);
    readonly IDocumentStore DocumentStore;

    public DefensiveRavenDBCheckpointReader(IDocumentStore documentStore)
    {
        DocumentStore = documentStore;
    }

    public async Task<Checkpoint> Read(string id)
    {
        int retryCount = 0;
        for (;;)
        {
            try
            {
                using (var s = DocumentStore.OpenAsyncSession())
                {
                    var chp = await s.LoadAsync<Checkpoint>(id).ConfigureAwait(false);
                    if (null == chp)
                        chp = new Checkpoint { Id = id, Value = 0 };
                    return chp;
                }
            }
            catch (Exception ex)
            {
                if (!IsTransient(ex) || (retryCount >= MaxRetries))
                    throw;

                retryCount++;
            }
            await Task.Delay(Delay).ConfigureAwait(false);
        }
    }

        bool IsTransient(Exception ex)
        {
            return (ex is Raven.Client.Exceptions.RavenException);
        }
}