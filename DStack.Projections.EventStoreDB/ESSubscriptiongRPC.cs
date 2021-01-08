using EventStore.Client;
using EventStore.ClientAPI;
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
        const int MaxRecconectionAttempts = 10;

        readonly ILogger<ESSubscription> Logger;
        readonly JsonSerializerSettings SerializerSettings;

        long CurrentCheckpoint = 0;

        EventStoreClient Client;

        int ReconnectionCounter;

        public string StreamName { get; set; }
        public Func<object, long, Task> EventAppearedCallback { get; set; }

        public ESSubscriptiongRPC(ILogger<ESSubscription> logger, EventStoreClient client)
        {
            Logger = logger;
            Client = client;
            SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        }

            object TryDeserializeEvent(byte[] metadata, byte[] data)
            {
                var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(metadata)).Property(EventClrTypeHeader).Value;
                var jsonString = Encoding.UTF8.GetString(data);
                try
                {
                    return JsonConvert.DeserializeObject(jsonString, Type.GetType((string)eventClrTypeName), SerializerSettings);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Failed to deserialize type: {eventClrTypeName}");
                    throw;
                }
            }

        public async Task Start(long fromCheckpoint)
        {

            if (fromCheckpoint == 0)
                await Client.SubscribeToStreamAsync(StreamName, EventAppeared, resolveLinkTos: true, SubDropped);
            else
                await Client.SubscribeToStreamAsync(StreamName, EventStore.Client.StreamPosition.FromInt64(fromCheckpoint - 1), EventAppeared, resolveLinkTos: true, SubDropped);

            ReconnectionCounter = 0;
        }

        private void SubDropped(StreamSubscription sub, SubscriptionDroppedReason reason, Exception ex)
        {
            if (reason == SubscriptionDroppedReason.ServerError)
            {
                ReconnectionCounter++;
                if (ReconnectionCounter > MaxRecconectionAttempts)
                    LogAndFail();
                Start(CurrentCheckpoint).Wait();
            }
            else
            {
                Logger.LogCritical(ex, $"{StreamName} subscription failed: ({reason}).");
                throw ex;
            }
        }

        private async Task EventAppeared(StreamSubscription arg1, EventStore.Client.ResolvedEvent @event, CancellationToken arg3)
        {
            CurrentCheckpoint = @event.OriginalEventNumber.ToInt64();
            var ev = TryDeserializeEvent(@event.Event.Metadata.ToArray(), @event.Event.Data.ToArray());
            await EventAppearedCallback(ev, CurrentCheckpoint + 1);
        }


            void LogAndFail()
            {
                var msg = $"Reconnection for {StreamName} subscription failed after {MaxRecconectionAttempts} attempts.";
                Logger.LogCritical(msg);
                throw new ApplicationException(msg);
            }
    }
}