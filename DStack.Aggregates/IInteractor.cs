using System.Collections.Generic;
using System.Threading.Tasks;

namespace DStack.Aggregates
{
    public interface IInteractor
    {
        Task Execute(object command);
        List<object> GetPublishedEvents();
    }

    public abstract class Interactor : IInteractor
    {
        protected List<object> PublishedEvents;

        public abstract Task Execute(object command);

        public Interactor()
        {
            PublishedEvents = new List<object>();
        }

        public List<object> GetPublishedEvents()
        {
            return PublishedEvents;
        }
    }
}