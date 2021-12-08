using Raven.Client.Documents;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DStack.Projections.RavenDB
{
    public class RavenDBProjectionsStore : INoSqlStore, ISqlStore
    {
        readonly IDocumentStore DocumentStore;

        public RavenDBProjectionsStore(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

        public async Task<T> LoadAsync<T>(string id) where T : class
        {
            using (var s = DocumentStore.OpenAsyncSession())
                return await s.LoadAsync<T>(id);
        }

        public async Task StoreAsync(object doc)
        {
            using (var s = DocumentStore.OpenAsyncSession())
            {
                await s.StoreAsync(doc);
                await s.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(string id)
        {
            using (var s = DocumentStore.OpenAsyncSession())
            {
                s.Delete(id);
                await s.SaveChangesAsync();
            }
        }

        public async Task StoreAsync<T>(T doc)
        {
            using (var s = DocumentStore.OpenAsyncSession())
            {
                await s.StoreAsync(doc);
                await s.SaveChangesAsync();
            }
        }

        public async Task StoreInUnitOfWorkAsync(params object[] docs)
        {
            using (var s = DocumentStore.OpenAsyncSession())
            {
                foreach (var d in docs)
                    await s.StoreAsync(d);
                await s.SaveChangesAsync();
            }
        }

        public async Task StoreInUnitOfWorkAsync<T>(params T[] docs)
        {
            using (var s = DocumentStore.OpenAsyncSession())
            {
                foreach (var d in docs)
                    await s.StoreAsync(d);
                await s.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<string, T>> LoadAsync<T>(params string[] ids) where T : class
        {
            using (var s = DocumentStore.OpenAsyncSession())
                return await s.LoadAsync<T>(ids);
        }

        public async Task DeleteInUnitOfWorkAsync(params string[] ids)
        {
            using (var s = DocumentStore.OpenAsyncSession())
            {
                foreach(var id in ids)
                    s.Delete(id);
                await s.SaveChangesAsync();
            }
        }
    }
}