using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Extensions.Context
{
    [BsonNoId]
    internal readonly struct SessionId
    {
        internal SessionId(Guid id)
        {
            Id = id;
        }

        [BsonId]
        [BsonElement("id")]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; }
    }
}
