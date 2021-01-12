using System;
using System.Linq;

namespace DStack.Projections.EventStoreDB.IntegrationTests
{
    public class TestEvent
    {
        public string Id { get; set; }
        public string SomeValue { get; set; }
    }
}