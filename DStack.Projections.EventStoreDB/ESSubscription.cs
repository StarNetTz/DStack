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
    public class ESSubscription : ISubscription
    {
        const string EventClrTypeHeader = "EventClrTypeName";
        const int MaxRecconectionAttemts = 10;

        readonly ILogger<ESSubscription> Logger;
        readonly JsonSerializerSettings SerializerSettings;

        long CurrentCheckpoint = 0;
        IEventStoreConnection Connection;
        EventStoreStreamCatchUpSubscription Subscription = null;
        int ReconnectionCounter;

        public string StreamName { get; set; }
        public Func<object, long, Task> EventAppearedCallback { get; set; }

        public ESSubscription(ILogger<ESSubscription> logger)
        {
            Logger = logger;
            SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        }

        public async Task EventAppeared(EventStoreCatchUpSubscription subscription, ResolvedEvent @event)
        {
            CurrentCheckpoint = @event.OriginalEventNumber;
            var ev = TryDeserializeEvent(@event.Event.Metadata, @event.Event.Data);
            await EventAppearedCallback(ev, @event.OriginalEventNumber + 1);
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
            Connection = EventStoreConnection.Create(EventStoreDBConfig.ConnectionString);
            Connection.Connected += Connection_Connected;
            await Connection.ConnectAsync().ConfigureAwait(false);
            long? eventstoreCheckpoint = (fromCheckpoint == 0) ? null : (long?)(fromCheckpoint - 1);
            Subscription = Connection.SubscribeToStreamFrom(StreamName, eventstoreCheckpoint, CatchUpSubscriptionSettings.Default, EventAppeared, LiveProcessingStarted, SubscriptionDropped);
        }

        void Connection_Connected(object sender, ClientConnectionEventArgs e)
        {
            ReconnectionCounter = 0;
            Logger.LogDebug($"Connected for {StreamName}");
        }

        void LiveProcessingStarted(EventStoreCatchUpSubscription obj)
        {
            Logger.LogDebug($"LiveProcessingStarted on stream {StreamName}");
        }

        void SubscriptionDropped(EventStoreCatchUpSubscription projection, SubscriptionDropReason subscriptionDropReason, Exception exception)
        {
            Connection.Connected -= Connection_Connected;
            Subscription.Stop();
            if (IsTransient(subscriptionDropReason))
            {
                ReconnectionCounter++;
                if (ReconnectionCounter > MaxRecconectionAttemts)
                    LogAndFail();

                Logger.LogWarning(exception, $"{StreamName} subscription dropped because of an transient error: ({subscriptionDropReason}). Reconnection attempt nr: {ReconnectionCounter}.");
                Thread.Sleep(300);
                Start(CurrentCheckpoint).Wait();
            }
            else
            {
                Logger.LogCritical(exception, $"{StreamName} subscription failed: ({subscriptionDropReason}).");
                throw exception;
            }
        }

            bool IsTransient(SubscriptionDropReason subscriptionDropReason)
            {
                switch (subscriptionDropReason)
                {
                    case SubscriptionDropReason.SubscribingError:
                    case SubscriptionDropReason.ServerError:
                    case SubscriptionDropReason.ConnectionClosed:
                    case SubscriptionDropReason.CatchUpError:
                    case SubscriptionDropReason.ProcessingQueueOverflow:
                        return true;
                    case SubscriptionDropReason.EventHandlerException:
                    case SubscriptionDropReason.UserInitiated:
                    default:
                        return false;
                }
            }

            void LogAndFail()
            {
                var msg = $"Reconnection attempt on {StreamName} subscription failed permanently. Limit({MaxRecconectionAttemts}) reached.";
                Logger.LogCritical(msg);
                throw new ApplicationException(msg);
            }
    }
}