namespace DStack.Projections.EventStoreDB.IntegrationTests
{
    [SubscribesToStream(TestProjection.StreamName)]
    public class TestProjection : Projection, IHandledBy<TestHandler> {
        public const string StreamName = "$ce-TestEvents";
    }
}
