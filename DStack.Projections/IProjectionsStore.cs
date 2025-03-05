using System.Collections.Generic;
using System.Threading.Tasks;

namespace DStack.Projections;

public interface IProjectionsStore
{
    Task<T> LoadAsync<T>(string id) where T : class;
    Task<Dictionary<string, T>> LoadAsync<T>(params string[] ids) where T : class;
    Task<T> LoadAsync<T>(object id) where T : class;
    Task<Dictionary<object, T>> LoadAsync<T>(params object[] ids) where T : class;
    Task StoreAsync(object doc);
    Task StoreInUnitOfWorkAsync(params object[] docs);
    Task StoreAsync<T>(T doc);
    Task StoreInUnitOfWorkAsync<T>(params T[] docs);
    Task DeleteAsync(string id);
    Task DeleteInUnitOfWorkAsync(params string[] ids);
}

public interface INoSqlStore : IProjectionsStore { };
public interface ISqlStore : IProjectionsStore { };