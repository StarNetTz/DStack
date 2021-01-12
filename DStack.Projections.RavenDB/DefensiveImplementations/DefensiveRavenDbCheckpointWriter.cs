using Raven.Client.Documents;
using System;
using System.Threading.Tasks;

namespace DStack.Projections.RavenDB
{
    public class DefensiveRavenDBCheckpointWriter : ICheckpointWriter
    {
        private int MaxRetries = 3;
        readonly TimeSpan Delay = TimeSpan.FromMilliseconds(50);

        readonly IDocumentStore DocumentStore;

        public DefensiveRavenDBCheckpointWriter(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

        public async Task Write(Checkpoint checkpoint)
        {
            int retryCount = 0;
            for (;;)
            {
                try
                {
                    using (var s = DocumentStore.OpenAsyncSession())
                    {
                        await s.StoreAsync(checkpoint);
                        await s.SaveChangesAsync();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    if (!IsTransient(ex) || (retryCount >= MaxRetries))
                        throw;

                    retryCount++;
                }
                await Task.Delay(Delay);
            }
        }

            bool IsTransient(Exception ex)
            {
                return (ex is Raven.Client.Exceptions.RavenException);
            }
    }
}