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
    public Func<object, ulong, Task> EventAppearedCallback { get; set; }

    public ESSubscription(ILogger<ESSubscription> logger, EventStoreClient client)
    {
        Logger = logger;
        Client = client;
        SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
    }

    public async Task StartAsync(ulong oneBasedCheckpoint)
    {
        if (oneBasedCheckpoint == 0)
            await Client.SubscribeToStreamAsync(StreamName, FromStream.Start, EventAppeared, resolveLinkTos: true, SubDropped).ConfigureAwait(false);
        else
            await Client.SubscribeToStreamAsync(StreamName, FromStream.After(new StreamPosition(oneBasedCheckpoint - 1)), EventAppeared, resolveLinkTos: true, SubDropped).ConfigureAwait(false);
        Logger.LogInformation($"Subscription started on stream: {StreamName}");
    }

        async Task EventAppeared(StreamSubscription sub, ResolvedEvent @event, CancellationToken tok)
        {
            ulong zeroBasedEventNumber = @event.OriginalEventNumber.ToUInt64();
            var ev = DeserializeEvent(@event.Event.Metadata.ToArray(), @event.Event.Data.ToArray());
            await EventAppearedCallback(ev, ConvertToOneBasedCheckpoint(zeroBasedEventNumber)).ConfigureAwait(false);
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
            Logger.LogCritical(ex, $"{StreamName} subscription failed: ({reason}).");
            throw ex;
        }
}