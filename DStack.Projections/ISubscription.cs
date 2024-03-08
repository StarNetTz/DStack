using System;
using System.Threading.Tasks;

namespace DStack.Projections;

public interface ISubscription
{
    string Name { get; set; }
    string StreamName { get; set; }
    Func<object, ulong, Task> EventAppearedCallback { get; set; }
    Task StartAsync(ulong fromCheckpoint);
}