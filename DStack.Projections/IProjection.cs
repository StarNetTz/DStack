using System.Collections.Generic;
using System.Threading.Tasks;

namespace DStack.Projections
{
    public interface IProjection
    {
        string Name { get; set; }
        ISubscription Subscription { get; set; }
        IEnumerable<IHandler> Handlers { get; set; }
        Checkpoint Checkpoint { get; set; }
        Task Project(object e, ulong c);
        Task Start();
    }
}
