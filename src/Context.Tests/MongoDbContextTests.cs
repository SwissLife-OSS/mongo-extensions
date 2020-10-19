using MongoDB.Bson;
using MongoDB.Driver;
using Squadron;
using Xunit;

namespace MongoDB.Extensions.Context.Tests
{
    public class MongoDbContextTests : IClassFixture<MongoResource>
    {
        private readonly MongoOptions _mongoOptions;
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDbContextTests(MongoResource mongoResource)
        {
            _mongoDatabase = mongoResource.CreateDatabase();
            _mongoOptions = new MongoOptions
            {
                ConnectionString = mongoResource.ConnectionString,
                DatabaseName = _mongoDatabase.DatabaseNamespace.DatabaseName
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_DatabaseAccess_InitializationExecuted()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);

            // Act
            _ = testMongoDbContext.Database;

            // Assert
            Assert.True(testMongoDbContext.IsInitialized);
        }

        [Fact]
        public void Constructor_CreateCollection_InitializationExecuted()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);

            // Act
            testMongoDbContext.CreateCollection<BsonDocument>();

            // Assert
            Assert.True(testMongoDbContext.IsInitialized);
        }

        [Fact]
        public void Constructor_ClientAccess_InitializationExecuted()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);

            // Act
            _ = testMongoDbContext.Client;

            // Assert
            Assert.True(testMongoDbContext.IsInitialized);
        }

        [Fact]
        public void Constructor_ClientAccess_InitializationNotExecuted()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);

            // Act
            _ = testMongoDbContext.MongoOptions;

            // Assert
            Assert.False(testMongoDbContext.IsInitialized);
        }

        #endregion

        #region Private Helpers

        private class TestMongoDbContext : MongoDbContext
        {
            public TestMongoDbContext(MongoOptions mongoOptions) : base(mongoOptions)
            {
            }

            protected override void OnConfiguring(IMongoDatabaseBuilder mongoDatabaseBuilder)
            {
                IsInitialized = true;
            }

            public bool IsInitialized { get; private set; }
        }

        #endregion
    }
}
