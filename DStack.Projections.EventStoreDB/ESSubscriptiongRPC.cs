using EventStore.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace DStack.Projections.EventStoreDB
{
    public class ESSubscriptiongRPC : ISubscription
    {
        const string EventClrTypeHeader = "EventClrTypeName";

        readonly ILogger<ESSubscriptiongRPC> Logger;
        readonly JsonSerializerSettings SerializerSettings;

        EventStoreClient Client;

        public string StreamName { get; set; }
        public Func<object, long, Task> EventAppearedCallback { get; set; }

        public ESSubscriptiongRPC(ILogger<ESSubscriptiongRPC> logger, EventStoreClient client)
        {
            Logger = logger;
            Client = client;
            SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        }

        public async Task Start(long oneBasedCheckpoint)
        {

            if (oneBasedCheckpoint == 0)
                await Client.SubscribeToStreamAsync(StreamName, EventAppeared, resolveLinkTos: true, SubDropped);
            else
                await Client.SubscribeToStreamAsync(StreamName, StreamPosition.FromInt64(oneBasedCheckpoint - 1), EventAppeared, resolveLinkTos: true, SubDropped);
            Logger.LogInformation($"Subscription started on stream: {StreamName}");
        }

            async Task EventAppeared(StreamSubscription sub, ResolvedEvent @event, CancellationToken tok)
            {
                long zeroBasedEventNumber = @event.OriginalEventNumber.ToInt64();
                var ev = DeserializeEvent(@event.Event.Metadata.ToArray(), @event.Event.Data.ToArray());
                await EventAppearedCallback(ev, ConvertToOneBasedCheckpoint(zeroBasedEventNumber));
            }

                object DeserializeEvent(byte[] metadata, byte[] data)
                {
                    var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(metadata)).Property(EventClrTypeHeader).Value;
                    var jsonString = Encoding.UTF8.GetString(data);
                    return JsonConvert.DeserializeObject(jsonString, Type.GetType((string)eventClrTypeName), SerializerSettings);
                }

                long ConvertToOneBasedCheckpoint(long zeroBasedCheckpoint)
                {
                    return zeroBasedCheckpoint + 1;
                }

            void SubDropped(StreamSubscription sub, SubscriptionDroppedReason reason, Exception ex)
            {
                Logger.LogCritical(ex, $"{StreamName} subscription failed: ({reason}).");
                throw ex;
            }
    }
}