using System.Threading.Tasks;

namespace DStack.Projections;

public interface ICheckpointReader
{
    Task<Checkpoint> Read(string id);
}
