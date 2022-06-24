using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DStack.Projections
{
    public class InMemorySubscription : ISubscription
    {
        Dictionary<ulong, object> EventStream = new Dictionary<ulong, object>();

        public string StreamName { get; set; }

        public Func<object, ulong, Task> EventAppearedCallback { get; set; }

        public void LoadEvents(params object[] events)
        {
            EventStream = new Dictionary<ulong, object>();
            ulong i = 0;
            foreach (var e in events)
                EventStream.Add(++i, e);
        }

        public async Task StartAsync(ulong fromCheckpoint)
        {
            foreach (var kv in EventStream)
                if (kv.Key >= fromCheckpoint)
                    await EventAppearedCallback(kv.Value, kv.Key).ConfigureAwait(false);
        }
    }
}
