namespace DStack.Projections.Tests;

[SubscribesToStream("$ce-Match")]
public class FailingProjection : Projection, IHandledBy<FailingHandler> { }