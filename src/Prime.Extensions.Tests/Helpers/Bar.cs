using System;

namespace MongoDB.Prime.Extensions.Tests;

public class Bar
{
    public Bar(Guid id, string name, string value)
    {
        Id = id;
        Name = name;
        Value = value;
    }

    public Guid Id { get; }

    public string Name { get; }
    public string Value { get; }
}
