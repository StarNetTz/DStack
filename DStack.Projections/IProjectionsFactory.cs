using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace DStack.Projections;

public interface IProjectionsFactory
{
    Task<IList<IProjection>> CreateAsync(Assembly projectionsAssembly);
    Task<IProjection> CreateAsync<T>();
    Task<IProjection> CreateAsync(Type type);
}