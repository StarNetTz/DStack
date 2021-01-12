using System;
using System.Threading.Tasks;

namespace DStack.Projections
{
    public interface ISubscription
    {
        string StreamName { get; set; }
        Func<object, long, Task> EventAppearedCallback { get; set; }
        Task Start(long fromCheckpoint);
    }
}