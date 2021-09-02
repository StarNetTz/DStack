using System.Linq;
using System.Threading.Tasks;

namespace DStack.Aggregates.Testing
{
    public class BDDAggregateRepository : InMemoryAggregateRepository
    {
        public object[] Appended = new object[0];

        public override Task StoreAsync(IAggregate agg)
        {
            var events = LoadEvents(agg.Id);
            events.AddRange(agg.Changes);
            DataStore[agg.Id] = events;
            Appended = agg.Changes.ToArray();
            agg.Changes.Clear();
            return Task.CompletedTask;
        }

        public void Preload(string id, object[] events)
        {
            if (events.Length > 0)
                DataStore[id] = events.ToList();
        }
    }
}