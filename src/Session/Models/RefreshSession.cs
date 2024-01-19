using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Extensions.Session
{
    internal class RefreshSession
    {
        internal RefreshSession(params SessionId[] sessionIds)
        {
            SessionIds = sessionIds;
        }

        [BsonElement("refreshSessions")]
        public SessionId[] SessionIds { get; }
    }
}
