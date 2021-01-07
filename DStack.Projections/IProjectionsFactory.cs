using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DStack.Projections
{
    public interface IProjectionsFactory
    {
        Task<IList<IProjection>> Create(Assembly projectionsAssembly);
        Task<IProjection> Create<T>();
        Task<IProjection> Create(Type type);
    }
}