using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Prime.Extensions.Tests;

public class Bar
{
    public Bar(Guid id, string name, string value)
    {
        Id = id;
        Name = name;
        Value = value;
    }

    [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
    public Guid Id { get; }

    public string Name { get; }
    public string Value { get; }
}
