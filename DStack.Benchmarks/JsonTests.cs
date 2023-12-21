using BenchmarkDotNet.Attributes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using BenchmarkDotNet.Running;

namespace DStack.Benchmarks
{
    [MemoryDiagnoser]
    public class JsonTests
    {
        const string EventClrTypeHeader = "EventClrTypeName";
        string metadata = "{\"$type\":\"System.Collections.Generic.Dictionary`2[[System.String, System.Private.CoreLib],[System.Object, System.Private.CoreLib]], System.Private.CoreLib\",\"CommitId\":\"9cfacc3e-50ea-49b2-bc36-9068c8484a32\",\"AggregateClrTypeName\":\"System.Reflection.RuntimeAssembly, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\",\"EventClrTypeName\":\"DStack.Projections.EventStoreDB.IntegrationTests.TestEvent, DStack.Projections.EventStoreDB.IntegrationTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"}";

        JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        string data = "{\"$type\":\"DStack.Projections.EventStoreDB.IntegrationTests.TestEvent, DStack.Projections.EventStoreDB.IntegrationTests\",\"Id\":\"TestEvents-5c2dcd0e-338b-42b5-923e-e906c174a76b\",\"SomeValue\":\"A guid: 7a9c79f4-eab3-4263-9198-25f21464ac54\"}";

        [Benchmark(Baseline = true)]
        public void ShouldDeserializeUsingNewtonsoft()
        {

            var eventClrTypeName = JObject.Parse(metadata).Property(EventClrTypeHeader).Value;
            var o = JsonConvert.DeserializeObject(data, Type.GetType((string)eventClrTypeName), SerializerSettings);
        }

        [Benchmark]
        public void ShouldDeserializeUsingSystemTextJson()
        {
            var jsonObj = JsonNode.Parse(metadata).AsObject();
            var nn = (string)jsonObj[EventClrTypeHeader];
            var o = System.Text.Json.JsonSerializer.Deserialize(data, Type.GetType(nn));
        }
    }
}
