using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Extensions.Transactions.Tests
{
    public class User
    {
        public User(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
        public Guid Id { get; }

        public string Name { get; }
    }
}
