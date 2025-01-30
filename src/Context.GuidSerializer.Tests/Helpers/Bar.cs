using System;

namespace MongoDB.Extensions.Context.GuidSerializers.Tests;

public class Bar
{
    public Bar(Guid fooId, string name, Guid additionalId)
    {
        Id = fooId;
        Name = name;
        AdditionalId = additionalId;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set;}

    public object AdditionalId { get; private set;}
}
