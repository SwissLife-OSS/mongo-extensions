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
        public void Constructor_AutoInitializeDefault_InitializationExecuted()
        {
            // Arrange
            
            // Act
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);

            // Assert
            Assert.True(testMongoDbContext.IsInitialized);
        }

        [Fact]
        public void Constructor_AutoInitializeManual_InitializationExecuted()
        {
            // Arrange
            
            // Act
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions, true);

            // Assert
            Assert.True(testMongoDbContext.IsInitialized);
        }

        [Fact]
        public void Constructor_NoInitializeManual_InitializationExecuted()
        {
            // Arrange

            // Act
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions, false);

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

            public TestMongoDbContext(MongoOptions mongoOptions, bool enableAutoInit) 
                : base(mongoOptions, enableAutoInit)
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
