
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

    public class RegisterPerson : ICommand
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class PersonRegistered : IEvent
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class RenamePerson : ICommand
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class PersonRenamed : IEvent
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
