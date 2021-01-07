using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DStack.Projections
{
    public class InMemorySubscription : ISubscription
    {
        Dictionary<long, object> EventStream = new Dictionary<long, object>();

        public string StreamName { get; set; }

        public Func<object, long, Task> EventAppearedCallback { get; set; }

        public void LoadEvents(params object[] events)
        {
            EventStream = new Dictionary<long, object>();
            long i = 0;
            foreach (var e in events)
                EventStream.Add(++i, e);
        }

        public async Task Start(long fromCheckpoint)
        {
            foreach (var kv in EventStream)
                if (kv.Key >= fromCheckpoint)
                    await EventAppearedCallback(kv.Value, kv.Key);
        }
    }
}
