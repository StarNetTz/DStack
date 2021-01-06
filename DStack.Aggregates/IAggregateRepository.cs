using System.Threading.Tasks;

namespace DStack.Aggregates
{
    public interface IAggregateRepository
    {
        Task StoreAsync(IAggregate aggregate);
        Task<TAggregate> GetAsync<TAggregate>(string id) where TAggregate : class, IAggregate;
        Task<TAggregate> GetAsync<TAggregate>(string id, int version) where TAggregate : class, IAggregate;
    }
}