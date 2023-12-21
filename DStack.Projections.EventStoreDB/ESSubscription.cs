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

    public string StreamName { get; set; }
    public bool HasFailed { get; private set; } = false;
    public string Error { get; private set; } = string.Empty;
    public Func<object, ulong, Task> EventAppearedCallback { get; set; }
    ulong CurrentCheckpoint;

    int ResubscriptionAttempt = 0;
    int RunningSubscriptionsCount = 0; 

    internal int MaxResubscriptionAttempts = 10;

    StreamSubscription CurrentSubscscription = null;

    public ESSubscription(ILogger<ESSubscription> logger, EventStoreClient client)
    {
        Logger = logger;
        Client = client;
        SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
    }

    public async Task StartAsync(ulong oneBasedCheckpoint)
    {
        if (RunningSubscriptionsCount > 0)
        {
            Logger.LogWarning($"Detected running subscription {CurrentSubscscription.SubscriptionId}. Refusing to start new one.");
            return;
        }
        CurrentCheckpoint = oneBasedCheckpoint;

        if (oneBasedCheckpoint == 0)
            CurrentSubscscription = await Client.SubscribeToStreamAsync(StreamName, FromStream.Start, EventAppeared, resolveLinkTos: true, SubDropped).ConfigureAwait(false);
        else
            CurrentSubscscription = await Client.SubscribeToStreamAsync(StreamName, FromStream.After(new StreamPosition(oneBasedCheckpoint - 1)), EventAppeared, resolveLinkTos: true, SubDropped).ConfigureAwait(false);
        Interlocked.Increment(ref RunningSubscriptionsCount);
        Logger.LogInformation($"Subscription started on stream: {StreamName}");
    }

        async Task EventAppeared(StreamSubscription sub, ResolvedEvent @event, CancellationToken tok)
        {
            ulong zeroBasedEventNumber = @event.OriginalEventNumber.ToUInt64();
            var ev = DeserializeEvent(@event.Event.Metadata.ToArray(), @event.Event.Data.ToArray());
            await EventAppearedCallback(ev, ConvertToOneBasedCheckpoint(zeroBasedEventNumber)).ConfigureAwait(false);
            if (ResubscriptionAttempt > 0)
                ResubscriptionAttempt = 0;
        }

            object DeserializeEvent(byte[] metadata, byte[] data)
            {
                var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(metadata)).Property(EventClrTypeHeader).Value;
                var jsonString = Encoding.UTF8.GetString(data);
                return JsonConvert.DeserializeObject(jsonString, Type.GetType((string)eventClrTypeName), SerializerSettings);
            }

            ulong ConvertToOneBasedCheckpoint(ulong zeroBasedCheckpoint)
            {
                return zeroBasedCheckpoint + 1;
            }

        void SubDropped(StreamSubscription sub, SubscriptionDroppedReason reason, Exception ex)
        {
            Logger.LogError(ex, $"{StreamName} subscription {sub.SubscriptionId} dropped: ({reason}).");
            Interlocked.Decrement(ref RunningSubscriptionsCount);
            switch (reason)
            {
                case  SubscriptionDroppedReason.Disposed:
                    Logger.LogInformation($"Stream disposed: {StreamName}");
                    break;
                default:
                    try
                    {
                        sub.Dispose();
                        ResubscriptionAttempt++;
                        if (ResubscriptionAttempt < MaxResubscriptionAttempts)
                            StartAsync(CurrentCheckpoint).Wait();
                        else
                            throw ex;
                    }
                    catch (Exception rex)
                    {
                        HasFailed = true;
                        Error = rex.Message;
                        Logger.LogCritical(rex, $"Failed to resubscribe to {StreamName}");
                        throw;
                    }
                    break;
            }
        }
}