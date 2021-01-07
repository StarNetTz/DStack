using System.Threading.Tasks;

namespace DStack.Projections.Testing
{
    public class StubCheckpointWriterFactory : ICheckpointWriterFactory
    {
        public Task<ICheckpointWriter> Create()
        {
            return Task.FromResult(new StubCheckpointWriter() as ICheckpointWriter);
        }
    }
}
