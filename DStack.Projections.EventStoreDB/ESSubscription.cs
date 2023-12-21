using EventStore.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace DStack.Projections.EventStoreDB;

public class ESSubscription : ISubscription
{
    const string EventClrTypeHeader = "EventClrTypeName";

    readonly ILogger<ESSubscription> Logger;
    readonly JsonSerializerSettings SerializerSettings;

    EventStoreClient Client;
    public string Name { get; set; }
    public string StreamName { get; set; }
    public bool HasFailed { get; private set; } = false;
    public string Error { get; private set; } = string.Empty;
    public Func<object, ulong, Task> EventAppearedCallback { get; set; }

    ulong CurrentCheckpoint;

    int ResubscriptionAttempt = 0;

    internal int MaxResubscriptionAttempts = 5;

    StreamSubscription CurrentSubscscription = null;

    public ESSubscription(ILogger<ESSubscription> logger, EventStoreClient client)
    {
        Logger = logger;
        Client = client;
        SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
    }

    public async Task StartAsync(ulong oneBasedCheckpoint)
    {
        CurrentCheckpoint = oneBasedCheckpoint;

        if (CurrentCheckpoint == 0)
            CurrentSubscscription = await Client.SubscribeToStreamAsync(StreamName, FromStream.Start, EventAppeared, resolveLinkTos: true, SubDropped).ConfigureAwait(false);
        else
            CurrentSubscscription = await Client.SubscribeToStreamAsync(StreamName, FromStream.After(new StreamPosition(CurrentCheckpoint - 1)), EventAppeared, resolveLinkTos: true, SubDropped).ConfigureAwait(false);
        
        Logger.LogInformation($"Projection {Name} on stream {StreamName} using subscription {CurrentSubscscription.SubscriptionId} started.");
    }

        async Task EventAppeared(StreamSubscription sub, ResolvedEvent @event, CancellationToken tok)
        {
            ulong oneBasedCheckPoint = @event.OriginalEventNumber.ToUInt64() + 1;
            var ev = DeserializeEvent(@event.Event.Metadata.ToArray(), @event.Event.Data.ToArray());
            await EventAppearedCallback(ev, oneBasedCheckPoint).ConfigureAwait(false);
            CurrentCheckpoint = oneBasedCheckPoint;

            if (ResubscriptionAttempt > 0)
                ResubscriptionAttempt = 0;
        }

            object DeserializeEvent(byte[] metadata, byte[] data)
            {
                var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(metadata)).Property(EventClrTypeHeader).Value;
                var jsonString = Encoding.UTF8.GetString(data);
                return JsonConvert.DeserializeObject(jsonString, Type.GetType((string)eventClrTypeName), SerializerSettings);
            }


        void SubDropped(StreamSubscription sub, SubscriptionDroppedReason reason, Exception ex)
        {
            Logger.LogError(ex, $"Projection {Name} on stream {StreamName} using subscription {CurrentSubscscription.SubscriptionId} dropped. Reason: ({reason}).");
            switch (reason)
            {
                case  SubscriptionDroppedReason.Disposed:
                    Logger.LogInformation($"Subscription disposed: {sub.SubscriptionId} {StreamName} {Name}");
                    break;
                default:
                    try
                    {
                        sub.Dispose();
                        CurrentSubscscription = null;
                        ResubscriptionAttempt++;
                        if (ResubscriptionAttempt < MaxResubscriptionAttempts)
                        {
                            Task.Delay(200).Wait();
                            StartAsync(CurrentCheckpoint).Wait();
                        }
                    else
                        throw ex;
                    }
                    catch (Exception rex)
                    {
                        HasFailed = true;
                        Error = rex.Message;
                        Logger.LogCritical(rex, $"Failed to resubscribe projection: {Name}-{StreamName}");
                        throw;
                    }
                    break;
            }
        }
}