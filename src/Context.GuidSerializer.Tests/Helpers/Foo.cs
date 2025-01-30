using System;

namespace MongoDB.Extensions.Context.GuidSerializers.Tests;

public class Foo
{
    public Foo(Guid fooId, string name, Guid additionalId)
    {
        Id = fooId;
        Name = name;
        AdditionalId = additionalId;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set;}

    public Guid AdditionalId { get; private set;}
}
