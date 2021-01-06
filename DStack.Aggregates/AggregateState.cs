namespace DStack.Aggregates
{
    public abstract class AggregateState : IAggregateState
    {
        public string Id { get; protected set; }
        public int Version { get; private set; }

        public void Mutate(object @event)
        {
            DelegateWhenToConcreteClass(@event);
            Version++;
        }

        protected abstract void DelegateWhenToConcreteClass(object ev);
    }
}