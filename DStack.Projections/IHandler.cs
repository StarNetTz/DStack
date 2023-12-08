using System.Threading.Tasks;

namespace DStack.Projections;

public interface IHandler
{
    Task Handle(dynamic @event, ulong checkpoint);
}
