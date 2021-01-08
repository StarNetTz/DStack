namespace DStack.Projections.ES.IntegrationTests
{
    [SubscribesToStream("$ce-TestEvents")]
    public class TestProjection : Projection, IHandledBy<TestHandler> { }
}
