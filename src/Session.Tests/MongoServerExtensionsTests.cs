using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Snapshooter.Xunit;
using Squadron;
using Xunit;
using Xunit.Abstractions;

namespace MongoDB.Extensions.Session.Tests
{
    public class MongoServerExtensionsTests : IClassFixture<MongoResource>
    {
        private readonly IMongoClient _mongoClient;
        
        public MongoServerExtensionsTests(MongoResource mongoResource)
        {
            _mongoClient = mongoResource.Client;
        }

        [Fact]
        public async Task GiveSession_WhenRefresh_ThenOkResult()
        {
            // Arrange
            IClientSessionHandle session = await _mongoClient.StartSessionAsync();

            // Act
            BsonDocument result = await _mongoClient
                .RefreshSessionAsync(session.GetSessionId(), default);

            // Assert
            result.MatchSnapshot();
        }

        [Fact]
        public async Task GiveSession_WhenRefresh_ThanLastUseIsUpdated()
        {
            // Arrange
            IClientSessionHandle session = await _mongoClient.StartSessionAsync();
            Guid sessionId = session.GetSessionId();

            List<ServerSession> serverSessionsFirst = await GetServerSessions();

            // Act
            await Task.Delay(TimeSpan.FromSeconds(1));
            _ = await _mongoClient.RefreshSessionAsync(session.GetSessionId(), default);
            List<ServerSession> serverSessionsSecond = await GetServerSessions();

            // Assert
            ServerSession sessionFirstUse = serverSessionsFirst
                .First(s => s.Id == sessionId);
            ServerSession sessionSecondUse = serverSessionsSecond
                .First(s => s.Id == sessionId);
            Assert.True(sessionSecondUse.LastUse > sessionFirstUse.LastUse);
        }

        [Fact]
        public async Task GiveSession_WhenNoRefresh_ThanLastUseIsSame()
         {
            // Arrange
            IClientSessionHandle session = await _mongoClient.StartSessionAsync();
            Guid sessionId = session.GetSessionId();

            List<ServerSession> serverSessionsFirst = await GetServerSessions();

            // Act
            await Task.Delay(TimeSpan.FromSeconds(1));
            List<ServerSession> serverSessionsSecond = await GetServerSessions();

            // Assert
            ServerSession sessionFirstUse = serverSessionsFirst
                .First(s => s.Id == sessionId);
            ServerSession sessionSecondUse = serverSessionsSecond
                .First(s => s.Id == sessionId);

            Assert.Equal(sessionSecondUse.LastUse, sessionFirstUse.LastUse);
        }

        private async Task<List<ServerSession>> GetServerSessions()
        {
            var pipeline = PipelineDefinition<NoPipelineInput, ServerSession>
                .Create("{ $listLocalSessions: { allUsers: true } }");

            return await _mongoClient                
                .GetDatabase("config")
                .WithReadConcern(ReadConcern.Local)
                .Aggregate(pipeline)
                .ToListAsync();
        }

        [BsonNoId]
        [BsonIgnoreExtraElements(true)]
        internal sealed class ServerSessionId
        {
            [BsonId]
            [BsonElement("id")]
            public Guid Id { get; set; }

            public static implicit operator Guid(ServerSessionId serverSessionId)
            {
                return serverSessionId.Id;
            }
        }

        internal sealed class ServerSession
        {
            [BsonId]
            [BsonElement("_id")]
            public ServerSessionId Id { get; set; }

            [BsonElement("lastUse")]
            public DateTime LastUse { get; set; }
        }
    }
}
