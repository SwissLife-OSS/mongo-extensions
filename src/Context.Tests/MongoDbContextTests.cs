using System;
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
        public void Constructor_AutoInitialize_InitializationExecuted()
        {
            // Arrange

            // Act
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);

            // Assert
            Assert.True(testMongoDbContext.IsInitialized);
        }

        [Fact]
        public void Constructor_NoInitialize_InitializationNotExecuted()
        {
            // Arrange

            // Act
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions, false);

            // Assert
            Assert.False(testMongoDbContext.IsInitialized);
        }

        [Fact]
        public void Constructor_Database_ThrowsWhenNoInitialize()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions, false);

            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(
                () => _ = testMongoDbContext.Database);
        }

        [Fact]
        public void Constructor_CreateCollection_ThrowsWhenNotInitialize()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions, false);

            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(
                () => _ = testMongoDbContext.CreateCollection<BsonDocument>());
        }

        [Fact]
        public void Constructor_Client_ThrowsWhenNoInitialize()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions, false);

            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(
                () => _ = testMongoDbContext.Client);
        }

        [Fact]
        public void Constructor_MongoOptions_CanAccessWhenNotInitialize()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions, false);

            // Act
            MongoOptions mongoOptions = testMongoDbContext.MongoOptions;

            // Assert
            Assert.NotNull(mongoOptions);
        }

        #endregion

        #region IMongoClient Constructor Tests

        [Fact]
        public void Constructor_WithIMongoClient_AutoInitialize_InitializationExecuted()
        {
            // Arrange
            var mongoClient = new MongoClient(_mongoOptions.ConnectionString);
            var databaseName = _mongoDatabase.DatabaseNamespace.DatabaseName;

            // Act
            var testMongoDbContext = new TestMongoDbContextWithClient(mongoClient, databaseName);

            // Assert
            Assert.True(testMongoDbContext.IsInitialized);
            Assert.Equal(mongoClient, testMongoDbContext.Client);
            Assert.Equal(databaseName, testMongoDbContext.Database.DatabaseNamespace.DatabaseName);
        }

        [Fact]
        public void Constructor_WithIMongoClient_NoInitialize_InitializationNotExecuted()
        {
            // Arrange
            var mongoClient = new MongoClient(_mongoOptions.ConnectionString);
            var databaseName = _mongoDatabase.DatabaseNamespace.DatabaseName;

            // Act
            var testMongoDbContext = new TestMongoDbContextWithClient(mongoClient, databaseName, false);

            // Assert
            Assert.False(testMongoDbContext.IsInitialized);
        }

        [Fact]
        public void Constructor_WithIMongoClient_NullClient_ThrowsArgumentNullException()
        {
            // Arrange
            IMongoClient mongoClient = null!;
            var databaseName = _mongoDatabase.DatabaseNamespace.DatabaseName;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TestMongoDbContextWithClient(mongoClient, databaseName));
        }

        [Fact]
        public void Constructor_WithIMongoClient_NullDatabaseName_ThrowsArgumentNullException()
        {
            // Arrange
            var mongoClient = new MongoClient(_mongoOptions.ConnectionString);
            string databaseName = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TestMongoDbContextWithClient(mongoClient, databaseName));
        }

        [Fact]
        public void Constructor_WithIMongoClient_Database_AccessibleAfterInitialize()
        {
            // Arrange
            var mongoClient = new MongoClient(_mongoOptions.ConnectionString);
            var databaseName = _mongoDatabase.DatabaseNamespace.DatabaseName;
            var testMongoDbContext = new TestMongoDbContextWithClient(mongoClient, databaseName, false);

            // Act
            testMongoDbContext.Initialize();

            // Assert
            Assert.NotNull(testMongoDbContext.Database);
            Assert.Equal(databaseName, testMongoDbContext.Database.DatabaseNamespace.DatabaseName);
        }

        [Fact]
        public void Constructor_WithIMongoClient_CreateCollection_WorksAfterInitialize()
        {
            // Arrange
            var mongoClient = new MongoClient(_mongoOptions.ConnectionString);
            var databaseName = _mongoDatabase.DatabaseNamespace.DatabaseName;
            var testMongoDbContext = new TestMongoDbContextWithClient(mongoClient, databaseName, false);

            // Act
            testMongoDbContext.Initialize();
            var collection = testMongoDbContext.CreateCollection<BsonDocument>();

            // Assert
            Assert.NotNull(collection);
        }

        [Fact]
        public void Constructor_WithIMongoClient_MongoOptions_IsConfiguredCorrectly()
        {
            // Arrange
            var mongoClient = new MongoClient(_mongoOptions.ConnectionString);
            var databaseName = _mongoDatabase.DatabaseNamespace.DatabaseName;

            // Act
            var testMongoDbContext = new TestMongoDbContextWithClient(mongoClient, databaseName);

            // Assert
            Assert.NotNull(testMongoDbContext.MongoOptions);
            Assert.Equal(databaseName, testMongoDbContext.MongoOptions.DatabaseName);
            Assert.Equal("", testMongoDbContext.MongoOptions.ConnectionString); // Empty for IMongoClient path
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

        private class TestMongoDbContextWithClient : MongoDbContext
        {
            public TestMongoDbContextWithClient(IMongoClient mongoClient, string databaseName) 
                : base(mongoClient, databaseName)
            {
            }

            public TestMongoDbContextWithClient(IMongoClient mongoClient, string databaseName, bool enableAutoInit)
                : base(mongoClient, databaseName, enableAutoInit)
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
