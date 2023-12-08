using System;

namespace DStack.Projections;

public class ProjectionException : Exception
{
    public string ProjectionName { get; init; }
    public string SubscriptionStreamName { get; init; }
    public ulong Checkpoint { get; init; }
    public string EventTypeName { get; init; }

    /// <summary>Initializes a new instance of the <see cref="ProjectionException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
    public ProjectionException(string message, Exception innerException) : base(message, innerException)
    {}
}