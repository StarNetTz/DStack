using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DStack.Projections;

public class Projection : IProjection
{
    public string Name { get; set; }
    public string SubscriptionStreamName { get; set; }
    public ISubscription Subscription { get; set; }
    public IEnumerable<IHandler> Handlers { get; set; }
    public ICheckpointWriter CheckpointWriter { get; set; }

    public Checkpoint Checkpoint { get; set; }

    public async Task ProjectAsync(object e, ulong c)
    {
        await TryHandleEvent(e, c).ConfigureAwait(false);
    }

        async Task TryHandleEvent(object e, ulong c)
        {
            try
            {
                await HandleEvent(e, c).ConfigureAwait(false);
            }
            catch (AggregateException ae)
            {
                throw CreateProjectionException(e, c, ae);
            }
        }

            async Task HandleEvent(object e, ulong c)
            {
                Checkpoint.Value = c;
                Task.WaitAll(StartHandlingTasks(e, c));
                await CheckpointWriter.Write(Checkpoint).ConfigureAwait(false);
            }

                Task[] StartHandlingTasks(object e, ulong c)
                {
                    var tasks = new List<Task>();
                    foreach (var d in Handlers)
                        tasks.Add(d.Handle(e, c));
                    return tasks.ToArray();
                }

            ProjectionException CreateProjectionException(object e, ulong c, AggregateException ae)
            {
                var msg = $"Projection {Name} on stream {SubscriptionStreamName} failed on checkpoint {c} while trying to project {e.GetType().FullName}";
                return new ProjectionException(msg, ae) {
                    ProjectionName = Name,
                    EventTypeName = e.GetType().FullName,
                    Checkpoint = c,
                    SubscriptionStreamName = SubscriptionStreamName
                };
            }

    public async Task StartAsync()
    {
        await Subscription.StartAsync(Checkpoint.Value).ConfigureAwait(false);
    }
}