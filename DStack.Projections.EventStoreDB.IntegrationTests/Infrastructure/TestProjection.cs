namespace DStack.Projections.ES.IntegrationTests
{
    [SubscribesToStream("$ce-Match")]
    public class TestProjection : Projection, IHandledBy<TestHandler> { }
}
