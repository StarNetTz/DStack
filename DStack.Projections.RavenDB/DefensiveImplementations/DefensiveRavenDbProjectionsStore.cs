using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DStack.Projections.RavenDB
{
    public class DefensiveRavenDBProjectionsStore : INoSqlStore, ISqlStore
    {
        private int MaxRetries = 3;
        readonly TimeSpan Delay = TimeSpan.FromMilliseconds(50);

        readonly IDocumentStore DocumentStore;

        public DefensiveRavenDBProjectionsStore(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

        public async Task<T> LoadAsync<T>(string id) where T : class
        {
            return await DefensivelyLoad(async () =>
            {
                using (var s = DocumentStore.OpenAsyncSession())
                    return await s.LoadAsync<T>(id);
            });
        }

        public async Task<Dictionary<string, T>> LoadAsync<T>(params string[] ids) where T : class
        {
            return await DefensivelyLoad(async () =>
            {
                using (var s = DocumentStore.OpenAsyncSession())
                    return await s.LoadAsync<T>(ids);
            });
        }

        public async Task StoreAsync(object doc)
        {
            await DefensivelyStore(async () =>
            {
                using (var s = DocumentStore.OpenAsyncSession())
                {
                    await s.StoreAsync(doc);
                    await s.SaveChangesAsync();
                }
            });
        }

        public async Task StoreAsync<T>(T doc)
        {
            await DefensivelyStore(async () =>
            {
                using (var s = DocumentStore.OpenAsyncSession())
                {
                    await s.StoreAsync(doc);
                    await s.SaveChangesAsync();
                }
            });
        }

        public async Task StoreInUnitOfWorkAsync(params object[] docs)
        {
            await DefensivelyStore(async () =>
            {
                using (var s = DocumentStore.OpenAsyncSession())
                {
                    foreach (var d in docs)
                        await s.StoreAsync(d);
                    await s.SaveChangesAsync();
                }
            });
        }

        public async Task StoreInUnitOfWorkAsync<T>(params T[] docs)
        {
            await DefensivelyStore(async () =>
            {
                using (var s = DocumentStore.OpenAsyncSession())
                {
                    foreach (var d in docs)
                        await s.StoreAsync(d);
                    await s.SaveChangesAsync();
                }
            });
        }

        public Task DeleteAsync(string id)
        {
            using (var s = DocumentStore.OpenSession())
            {
                s.Delete(id);
                s.SaveChanges();
            }

            return Task.CompletedTask;
        }

        async Task<T> DefensivelyLoad<T>(Func<Task<T>> func)
        {
            int retryCount = 0;
            for (; ; )
            {
                try
                {
                    return await func();
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

        async Task DefensivelyStore(Func<Task> func)
        {
            int retryCount = 0;
            for (;;)
            {
                try
                {
                    await func();
                    return;
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