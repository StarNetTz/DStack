using System;
using System.Threading.Tasks;

namespace DStack.Projections.Testing
{
    public class StubCheckpointWriter : ICheckpointWriter
    {
        public Task Write(Checkpoint checkpoint)
        {
            return Task.CompletedTask;
        }
    }
}
