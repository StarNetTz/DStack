using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DStack.Projections.Testing;

public class InMemoryProjectionsStore : INoSqlStore, ISqlStore
{
    ConcurrentDictionary<string, object> Store = new ConcurrentDictionary<string, object>();

    public Task<T> LoadAsync<T>(string id) where T : class
    {
        object returnValue;
        if (!Store.TryGetValue(id, out returnValue))
            return Task.FromResult(default(T));
        return Task.FromResult(returnValue as T);
    }

    public Task StoreAsync(object doc)
    {
        var id = doc.GetType().GetProperty("Id").GetValue(doc, null);
        ValidateIdType(id);
        Store[id.ToString()] = doc;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        Store.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task StoreAsync<T>(T doc)
    {
        var id = doc.GetType().GetProperty("Id").GetValue(doc, null);
        ValidateIdType(id);
        Store[id.ToString()] = doc;
        return Task.CompletedTask;
    }

    void ValidateIdType(object id)
    {
        switch (id)
        {
            case string s:
            case int i:
            case long l:
            case Guid g:
                return;
            default:
                throw new ArgumentException("Unsopported Id type!");
        }
    }

    public async Task StoreInUnitOfWorkAsync(params object[] docs)
    {
       foreach (var d in docs)
            await StoreAsync(d).ConfigureAwait(false);
    }

    public async Task StoreInUnitOfWorkAsync<T>(params T[] docs)
    {
        foreach (var d in docs)
            await StoreAsync<T>(d).ConfigureAwait(false);
    }

    public async Task<Dictionary<string, T>> LoadAsync<T>(params string[] ids) where T : class
    {
        var data = new Dictionary<string, T>();
        foreach (var id in ids)
                data.Add(id, await LoadAsync<T>(id).ConfigureAwait(false));
        return data;
    }

    public Task DeleteInUnitOfWorkAsync(params string[] ids)
    {
        foreach (var id in ids)
            _ = Store.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<T> LoadAsync<T>(object id) where T : class
    {
        object returnValue;
        if (!Store.TryGetValue(id.ToString(), out returnValue))
            return Task.FromResult(default(T));
        return Task.FromResult(returnValue as T);
    }

    public async Task<Dictionary<object, T>> LoadAsync<T>(params object[] ids) where T : class
    {
        var data = new Dictionary<object, T>();
        foreach (var id in ids)
            data.Add(id, await LoadAsync<T>(id).ConfigureAwait(false));
        return data;
    }
}