using System.Collections.Generic;
using System.Threading.Tasks;

namespace DStack.Aggregates;

public interface IInteractor
{
    Task ExecuteAsync(object command);
    List<object> GetPublishedEvents();
}