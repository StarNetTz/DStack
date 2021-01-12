using System;

namespace DStack.Projections.UnitTests
{
    public interface ITimeProvider
    {
        DateTime GetUtcNow();
    }

    public class MockTimeProvider : ITimeProvider
    {
        public DateTime GetUtcNow()
        {
            return DateTime.MinValue;
        }
    }
}
