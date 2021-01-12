using EventStore.ClientAPI;
using System;
using System.Threading.Tasks;

namespace DStack.Projections.EventStoreDB.IntegrationTests
{
    public class StubCheckpointReader : ICheckpointReader
    {
        public Task<Checkpoint> Read(string id)
        {
            return Task.FromResult(new Checkpoint { Id = id, Value = 1 });
        }
    }

    public class StubHandlerFactory : IHandlerFactory
    {
        public IHandler Create(Type t)
        {
            return Activator.CreateInstance(t) as IHandler;
        }
    }

    public class StubCheckpointWriter : ICheckpointWriter
    {
        public Task Write(Checkpoint checkpoint)
        {
            return Task.CompletedTask;
        }
    }

    public class StubCheckpointWriterFactory : ICheckpointWriterFactory
    {
        public Task<ICheckpointWriter> Create()
        {
            return Task.FromResult(new StubCheckpointWriter() as ICheckpointWriter);
        }
    }
}
