using EventStore.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DStack.Aggregates.EventStoreDB;

public class ESAggregateRepository : IAggregateRepository
{
    const string EventClrTypeHeader = "EventClrTypeName";
    const string AggregateClrTypeHeader = "AggregateClrTypeName";
    const string CommitIdHeader = "CommitId";

    readonly EventStoreClient Client;
    readonly JsonSerializerSettings SerializerSettings;

    public ESAggregateRepository(EventStoreClient client)
    {
        SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        Client = client;
    }

    public async Task StoreAsync(IAggregate aggregate)
    {
        await TrySaveAggregate(aggregate).ConfigureAwait(false);
    }

        async Task TrySaveAggregate(IAggregate aggregate)
        {
            try
            {
                await SaveAggregate(aggregate, Guid.NewGuid(), (d) => { }).ConfigureAwait(false);
            }
            catch (WrongExpectedVersionException ex)
            {
                throw new ConcurrencyException(ex.Message);
            }
        }

    async Task SaveAggregate(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
    {
        var commitHeaders = new Dictionary<string, object>
        {
            {CommitIdHeader, commitId},
            {AggregateClrTypeHeader, aggregate.GetType().AssemblyQualifiedName}
        };
        updateHeaders(commitHeaders);

        var streamName = aggregate.Id;
        var newEvents = aggregate.Changes.Cast<object>().ToList();
        var originalVersion = aggregate.Version - newEvents.Count;
        var expectedRevision = originalVersion == 0 ? StreamRevision.None : StreamRevision.FromInt64(originalVersion - 1);
        var eventsToSave = newEvents.Select(e => ToEventData(e, commitHeaders)).ToList();
        await Client.AppendToStreamAsync(streamName, expectedRevision, eventsToSave).ConfigureAwait(false);

        aggregate.Changes.Clear();
    }

    EventData ToEventData(dynamic evnt, IDictionary<string, object> headers)
    {
        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evnt, SerializerSettings));
        var eventHeaders = new Dictionary<string, object>(headers)
        {
            {
                EventClrTypeHeader, evnt.GetType().AssemblyQualifiedName
            }
        };
        var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders, SerializerSettings));
        var typeName = evnt.GetType().Name;
        return new EventData(Uuid.NewUuid(), typeName, data, metadata);
    }

    public Task<TAggregate> GetAsync<TAggregate>(string id) where TAggregate : class, IAggregate
    {
        return GetAsync<TAggregate>(id, int.MaxValue);
    }

    public async Task<TAggregate> GetAsync<TAggregate>(string id, int version) where TAggregate : class, IAggregate
    {
        var streamName = id;
        Type aggregateType = typeof(TAggregate);
        var instanceOfState = AggregateStateFactory.CreateStateFor(aggregateType);
       

        var events = Client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start, version).ConfigureAwait(false);
        try
        {
            await foreach (var @event in events)
            {
                instanceOfState.Mutate(DeserializeEvent(@event.Event.Metadata.ToArray(), @event.Event.Data.ToArray()));
                if (instanceOfState.Version == version)
                    return Activator.CreateInstance(aggregateType, instanceOfState) as TAggregate;
            }
        }
        catch (StreamNotFoundException)
        {
            return null;
        }
        return Activator.CreateInstance(aggregateType, instanceOfState) as TAggregate;
    }

        object DeserializeEvent(byte[] metadata, byte[] data)
        {
            var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(metadata)).Property(EventClrTypeHeader).Value;
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType((string)eventClrTypeName), SerializerSettings);
        }
}