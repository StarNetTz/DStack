﻿using System;

namespace DStack.Projections
{
    public interface ISubscriptionFactory
    {
        ISubscription Create();
    }
}
