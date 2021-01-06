namespace DStack.Aggregates
{
    public interface IAggregateState
    {
        int Version { get; }

        void Mutate(object @event);

        string Id { get; }
    }
}