using EventStore.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DStack.Projections.EventStoreDB;

public class ESSubscription : ISubscription
{
    const string EventClrTypeHeader = "EventClrTypeName";

    readonly ILogger<ESSubscription> Logger;

    EventStoreClient Client;
    public string Name { get; set; }
    public string StreamName { get; set; }
    public bool HasFailed { get; private set; } = false;
    public string Error { get; private set; } = string.Empty;
    public Func<object, ulong, Task> EventAppearedCallback { get; set; }

    ulong CurrentCheckpoint;

    int ResubscriptionAttempt = 0;

    internal int MaxResubscriptionAttempts = 5;


    public ESSubscription(ILogger<ESSubscription> logger, EventStoreClient client)
    {
        Logger = logger;
        Client = client;
    }

    public async Task StartAsync(ulong oneBasedCheckpoint)
    {
        CurrentCheckpoint = oneBasedCheckpoint;
        var checkpoint = CurrentCheckpoint == 0 ? FromStream.Start : FromStream.After(CurrentCheckpoint - 1);
    Subscribe:
        try
        {
            
            await using var subscription = Client.SubscribeToStream(
                        StreamName,
                        checkpoint,
                        resolveLinkTos: true);
            Logger.LogInformation($"Projection {Name} on stream {StreamName} using subscription {subscription.SubscriptionId} started.");
            await foreach (var message in subscription.Messages)
            {
                switch (message)
                {
                    case StreamMessage.Event(var evnt):
                        await HandleEvent(evnt);
                        checkpoint = FromStream.After(evnt.OriginalEventNumber);
                        ResubscriptionAttempt = 0;
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation($"Subscription was canceled.");
        }
        catch (ObjectDisposedException)
        {
            Logger.LogInformation($"Subscription was canceled by the user.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Subscription was dropped: {ex}");
            ResubscriptionAttempt++;
            if (ResubscriptionAttempt < MaxResubscriptionAttempts)
            {
                goto Subscribe;
            }
            else
            {
                HasFailed = true;
                Error = ex.Message;
                Logger.LogCritical(ex, $"Failed to resubscribe projection: {Name}-{StreamName}");
                throw;
            }

        }
    }

    async Task HandleEvent(ResolvedEvent @event)
    {
        ulong oneBasedCheckPoint = @event.OriginalEventNumber.ToUInt64() + 1;
        if (IsNotDeleted(@event))
        {
            var ev = DeserializeEvent(@event.Event.Metadata.ToArray(), @event.Event.Data.ToArray());
            await EventAppearedCallback(ev, oneBasedCheckPoint).ConfigureAwait(false);
        }
        CurrentCheckpoint = oneBasedCheckPoint;
        if (ResubscriptionAttempt > 0)
            ResubscriptionAttempt = 0;
    }

        static bool IsNotDeleted(ResolvedEvent @event)
        {
            return @event.Event != null;
        }

    object DeserializeEvent(byte[] metadata, byte[] data)
    {
        var eventClrTypeName = (string)JsonNode.Parse(metadata)[EventClrTypeHeader];
        return System.Text.Json.JsonSerializer.Deserialize(data, Type.GetType(eventClrTypeName));
    }
}