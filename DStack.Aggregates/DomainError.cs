using System;

namespace DStack.Aggregates;

[Serializable]
public class DomainError : Exception
{
    public DomainError()
    {
    }

    public DomainError(string message) : base(message) { }

    public static DomainError Named(string name, string message)
    {
        return new DomainError(message)
        {
            Name = name
        };
    }

    public string Name { get; private set; }
}