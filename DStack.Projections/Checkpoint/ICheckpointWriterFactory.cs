using System.Threading.Tasks;

namespace DStack.Projections;

public interface ICheckpointWriterFactory
{
    Task<ICheckpointWriter> Create();
}
