using EventStore.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DStack.Projections.EventStoreDB.IntegrationTests;

public class Should
{

   [Fact]
   public async Task Should_initialize()
   {
        CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken ct = source.Token;
        var client = EventStoreClientFactory.CreateEventStoreClient(); 
        await using var subscription = client.SubscribeToAll(
     FromAll.Start, resolveLinkTos: true, cancellationToken:ct);
        await foreach (var message in subscription.Messages.WithCancellation(ct))
        {
            switch (message)
            {
                case StreamMessage.Event(var evnt):
                    Console.WriteLine($"Received event {evnt.OriginalEventNumber}@{evnt.OriginalStreamId}");
                    await HandleEvent(evnt);
                    break;
            }
        }
    }

    private async Task HandleEvent(ResolvedEvent evnt)
    {
       // throw new NotImplementedException();
    }
}
