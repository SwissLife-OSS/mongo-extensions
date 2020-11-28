using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Snapshooter.Xunit;
using Squadron;
using Xunit;

namespace MongoDB.Extensions.Context.Tests
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
    }
}
