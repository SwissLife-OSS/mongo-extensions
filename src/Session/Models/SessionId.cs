using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Extensions.Session
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
