using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context
{
    public static class MongoServerSessionExtensions
    {
        public static BsonDocument RefreshSession(
            this IMongoClient mongoClient,
            Guid sessionId)
        {
            return mongoClient.RefreshSessions(new[] { sessionId });
        }

        public static BsonDocument RefreshSessions(
            this IMongoClient mongoClient,
            IEnumerable<Guid> sessionIds)
        {
            ObjectCommand<BsonDocument> command = CreateRefreshCommand(sessionIds);

            return mongoClient
                .GetDatabase("admin")
                .RunCommand(command);
        }

        public static Task<BsonDocument> RefreshSessionAsync(
            this IMongoClient mongoClient,
            Guid sessionId,
            CancellationToken cancellationToken)
        {
            return mongoClient
                .RefreshSessionsAsync(new []{ sessionId }, cancellationToken);
        }

        public static Task<BsonDocument> RefreshSessionsAsync(
            this IMongoClient mongoClient,
            IEnumerable<Guid> sessionIds,
            CancellationToken cancellationToken)
        {
            ObjectCommand<BsonDocument> command = CreateRefreshCommand(sessionIds);

            return mongoClient
                .GetDatabase("admin")
                .RunCommandAsync(command, cancellationToken: cancellationToken);
        }

        private static ObjectCommand<BsonDocument> CreateRefreshCommand(
            IEnumerable<Guid> sessionIds)
        {
            SessionId[] sessions = sessionIds.Select(id => new SessionId(id)).ToArray();
            var refreshSession = new RefreshSession(sessions);

            return new ObjectCommand<BsonDocument>(refreshSession);
        }
    }
}
