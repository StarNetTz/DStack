
namespace DStack.Aggregates
{
    public interface ICommand
    {
        string Id { get; }
    }

    public interface IEvent
    {
        string Id { get; }
    }

    public record RegisterPerson : ICommand
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public record PersonRegistered : IEvent
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public record RenamePerson : ICommand
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public record PersonRenamed : IEvent
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
