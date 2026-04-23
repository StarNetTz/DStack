using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.KurrentDB.IntegrationTests;

public class KurrentInitializationTests
{

   [Fact]
   public async Task Should_initialize()
   {
        await new KurrentDataGenerator().WriteTestEventsToStore(200);
   }
}
