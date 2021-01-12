namespace DStack.Projections.Tests
{
    public class TestModel
    {
        public string Id { get; set; }
        public string SomeValue { get; set; }
    }

    public class TestModelWithUnsupportedIdType
    {
        public short Id { get; set; }
        public string SomeValue { get; set; }
    }
}
