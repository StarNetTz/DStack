// See https://aka.ms/new-console-template for more information
using DStack.Projections.EventStoreDB;
using DStack.Projections.EventStoreDB.IntegrationTests;
using EventStore.Client;
using EventStoreSubscriber;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

Console.WriteLine("Hello, World!");
ResolvedEvent evnt;

//await SubscribeManually();

var Subscription = new ESSubscription3(new NullLoggerFactory().CreateLogger<ESSubscription3>(), EventStoreClientFactory.CreateEventStoreClient())
{
    Name = nameof(TestProjection),
    StreamName = TestProjection.StreamName,
    EventAppearedCallback = EventAppeared
};

async Task EventAppeared(object arg1, ulong arg2)
{
    Console.WriteLine("Aloha");
}

await Subscription.StartAsync(0);

static async Task SubscribeManually()
{
   
    try
    {
        var client = EventStoreClientFactory.CreateEventStoreClient();
        await using var subscription = client.SubscribeToStream(TestProjection.StreamName, FromStream.Start, resolveLinkTos: true);
        await foreach (var message in subscription.Messages)
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
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }

    async Task HandleEvent(ResolvedEvent evnt)
    {
        Console.WriteLine(evnt.OriginalEvent.EventNumber);
    }
}
