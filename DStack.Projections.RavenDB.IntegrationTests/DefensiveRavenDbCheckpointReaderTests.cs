using System;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.RavenDB.IntegrationTests
{
    public class DefensiveRavenDBCheckpointReaderTests : IntegrationTestBase
    {
        [Fact]
        public async Task CanWriteAndReadCheckpoint()
        {
            var w = new RavenDBCheckpointWriterWithRetries(DocumentStore);
            var r = new DefensiveRavenDBCheckpointReader(DocumentStore);
            var id = $"Checkpoints-{Guid.NewGuid()}";
            await w.Write(new Checkpoint { Id = id, Value = 1001 });
            var chp = await r.Read(id);
            Assert.Equal(1001UL, chp.Value);
        }
    }
}
