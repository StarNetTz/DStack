using System;

namespace DStack.Projections
{
    public class InMemorySubscriptionFactory : ISubscriptionFactory
    {
        public ISubscription Create()
        {
            return new InMemorySubscription();
        }
    }
}
