using System;

namespace MongoDB.Extensions.Context.InterferingTests.Helpers
{
    public class Foo
    {
        public Foo(Guid fooId, string name, int number)
        {
            FooId = fooId;
            Name = name;
            Number = number;
            Timestamp = DateTime.UtcNow;
        }

        public Guid FooId { get; }

        public string Name { get; }

        public int Number { get; }

        public DateTime Timestamp { get; }
    }
}
