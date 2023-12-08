using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DStack.Projections.RavenDB;

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
                return await s.LoadAsync<T>(id).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public async Task<Dictionary<string, T>> LoadAsync<T>(params string[] ids) where T : class
    {
        return await DefensivelyLoad(async () =>
        {
            using (var s = DocumentStore.OpenAsyncSession())
                return await s.LoadAsync<T>(ids).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public async Task StoreAsync(object doc)
    {
        await DefensivelyStore(async () =>
        {
            using (var s = DocumentStore.OpenAsyncSession())
            {
                await s.StoreAsync(doc).ConfigureAwait(false);
                await s.SaveChangesAsync().ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    public async Task StoreAsync<T>(T doc)
    {
        await DefensivelyStore(async () =>
        {
            using (var s = DocumentStore.OpenAsyncSession())
            {
                await s.StoreAsync(doc).ConfigureAwait(false);
                await s.SaveChangesAsync().ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    public async Task StoreInUnitOfWorkAsync(params object[] docs)
    {
        await DefensivelyStore(async () =>
        {
            using (var s = DocumentStore.OpenAsyncSession())
            {
                foreach (var d in docs)
                    await s.StoreAsync(d).ConfigureAwait(false);
                await s.SaveChangesAsync().ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    public async Task StoreInUnitOfWorkAsync<T>(params T[] docs)
    {
        await DefensivelyStore(async () =>
        {
            using (var s = DocumentStore.OpenAsyncSession())
            {
                foreach (var d in docs)
                    await s.StoreAsync(d).ConfigureAwait(false);
                await s.SaveChangesAsync().ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    public async Task DeleteAsync(string id)
    {
        await DefensivelyStore(async () =>
        {
            using (var s = DocumentStore.OpenAsyncSession())
            {
                s.Delete(id);
                await s.SaveChangesAsync().ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    public async Task DeleteInUnitOfWorkAsync(params string[] ids)
    {
        await DefensivelyStore(async () =>
        {
            using (var s = DocumentStore.OpenAsyncSession())
            {
                foreach (var id in ids)
                    s.Delete(id);
                await s.SaveChangesAsync().ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    async Task<T> DefensivelyLoad<T>(Func<Task<T>> func)
    {
        int retryCount = 0;
        for (; ; )
        {
            try
            {
                return await func().ConfigureAwait(false);
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

    async Task DefensivelyStore(Func<Task> func)
    {
        int retryCount = 0;
        for (;;)
        {
            try
            {
                await func().ConfigureAwait(false);
                return;
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