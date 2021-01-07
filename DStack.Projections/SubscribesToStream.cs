using System;

namespace DStack.Projections
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class SubscribesToStream : Attribute
    {
        public string Name { get; set; }
        public SubscribesToStream(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class InactiveProjection : Attribute
    {
    }
}