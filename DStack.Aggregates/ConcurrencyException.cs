using System;

namespace DStack.Aggregates
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message) : base(message) {}
    }
}