using EventStore.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DStack.Projections.EventStoreDB.IntegrationTests;

public class ESDataGenerator
{
    private const string AggregateClrTypeHeader = "AggregateClrTypeName";
    private const string CommitIdHeader = "CommitId";

    public async Task WriteTestEventsToStore(int nrOfEvents)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, false).Build();
        var settings = EventStoreClientSettings.Create(configuration["EventStoreDB:ConnectionString"]);
        var client = new EventStoreClient(settings);

        for (int i = 0; i < nrOfEvents; i++)
        {
            var id = $"TestEvents-{Guid.NewGuid()}";
            await WriteEvent(client, id, new TestEvent() { Id = id, SomeValue = $"A guid: {Guid.NewGuid()}" });
        }
    }

    public static async Task WriteEvent(EventStoreClient cnn, string streamName, params object[] events)
    {
        var commitHeaders = new Dictionary<string, object>
                        {
                            {CommitIdHeader, Guid.NewGuid().ToString()},
                            {AggregateClrTypeHeader, Assembly.GetExecutingAssembly().GetType().AssemblyQualifiedName}
                        };
        var eventsToSave = events.Select(e => ToEventData(e, commitHeaders)).ToList();
        await cnn.AppendToStreamAsync(streamName, StreamRevision.None, eventsToSave);
    }

        static EventStore.Client.EventData ToEventData(dynamic evnt, IDictionary<string, object> headers)
        {

            var data = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(evnt));

            var eventHeaders = new Dictionary<string, object>(headers) {
                { "EventClrTypeName", evnt.GetType().AssemblyQualifiedName }
            };
            var metadata = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(eventHeaders));
            var typeName = evnt.GetType().Name;
            return new EventStore.Client.EventData(Uuid.NewUuid(), typeName, data, metadata);
        }
}