using System.Threading.Tasks;

namespace DStack.Projections;

public interface ICheckpointWriter
{
    Task Write(Checkpoint checkpoint);
}
