using KurrentDB.Client;
using System.Text;
using System.Text.Json.Nodes;

namespace DStack.Aggregates.KurrentDB;


public class KurrentAggregateRepository : IAggregateRepository
{
    const string EventClrTypeHeader = "EventClrTypeName";
    const string AggregateClrTypeHeader = "AggregateClrTypeName";
    const string CommitIdHeader = "CommitId";
    const int RepositoryOperationTimeoutInMiliseconds = 150;

    readonly KurrentDBClient Client;

    public KurrentAggregateRepository(KurrentDBClient client)
    {
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
        StreamState expectedRevision = originalVersion == 0 ? StreamState.NoStream : (ulong)(originalVersion - 1);
        var eventsToSave = newEvents.Select(e => ToEventData(e, commitHeaders)).ToList();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(RepositoryOperationTimeoutInMiliseconds));
        await Client.AppendToStreamAsync(streamName, expectedRevision, eventsToSave, cancellationToken: cts.Token).ConfigureAwait(false);

        aggregate.Changes.Clear();
    }

    EventData ToEventData(dynamic evnt, IDictionary<string, object> headers)
    {
        var data = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(evnt));
        var eventHeaders = new Dictionary<string, object>(headers)
        {
            {
                EventClrTypeHeader, evnt.GetType().AssemblyQualifiedName
            }
        };
        var metadata = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(eventHeaders));
        var typeName = evnt.GetType().Name;
        return new EventData(Uuid.NewUuid(), typeName, data, metadata);
    }

    public Task<TAggregate> GetAsync<TAggregate>(string id) where TAggregate : class, IAggregate, new()
    {
        return GetAsync<TAggregate>(id, int.MaxValue);
    }

    public async Task<TAggregate> GetAsync<TAggregate>(string id, int version) where TAggregate : class, IAggregate, new()
    {
        var streamName = id;
        Type aggregateType = typeof(TAggregate);
        var instanceOfState = AggregateStateFactory.CreateStateFor(aggregateType);

        var agg = new TAggregate();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(RepositoryOperationTimeoutInMiliseconds));
        var events = Client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start, version, cancellationToken: cts.Token).ConfigureAwait(false);
        try
        {
            await foreach (var @event in events.WithCancellation(cts.Token))
            {
                instanceOfState.Mutate(DeserializeEvent(@event.Event.Metadata.ToArray(), @event.Event.Data.ToArray()));

                if (instanceOfState.Version == version)
                {
                    agg.SetState(instanceOfState);
                    return agg;
                }
            }
        }
        catch (StreamNotFoundException)
        {
            return null;
        }

        agg.SetState(instanceOfState);
        return agg;
    }

    object DeserializeEvent(byte[] metadata, byte[] data)
    {
        var eventClrTypeName = (string)JsonNode.Parse(metadata)[EventClrTypeHeader];
        return System.Text.Json.JsonSerializer.Deserialize(data, Type.GetType(eventClrTypeName));
    }
}