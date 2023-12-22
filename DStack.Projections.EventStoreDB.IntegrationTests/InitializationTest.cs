using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.EventStoreDB.IntegrationTests;

public class ESInitializationTests
{

   [Fact]
   public async Task Should_initialize()
   {
        await new ESDataGenerator().WriteTestEventsToStore(200);
   }
}
