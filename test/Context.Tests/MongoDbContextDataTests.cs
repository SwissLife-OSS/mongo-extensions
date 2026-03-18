using MongoDB.Driver;
using MongoDB.Extensions.Context.Internal;
using Squadron;
using Xunit;

namespace MongoDB.Extensions.Context.Tests
{
    public class MongoDbContextDataTests : IClassFixture<MongoResource>
    {
        private readonly MongoOptions _mongoOptions;
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDbContextDataTests(MongoResource mongoResource)
        {
            _mongoDatabase = mongoResource.CreateDatabase();
            _mongoOptions = new MongoOptions
            {
                ConnectionString = mongoResource.ConnectionString,
                DatabaseName = _mongoDatabase.DatabaseNamespace.DatabaseName
            };
        }

        #region GetCollection Tests

        [Fact]
        public void GetCollection_GetFooCollection_ReturnsRightCollection()
        {
            // Arrange
            IMongoCollection<Foo> mongoCollectionFoo =
                _mongoDatabase.GetCollection<Foo>(nameof(Foo));
            IMongoCollection<Bar> mongoCollectionBar =
                _mongoDatabase.GetCollection<Bar>(nameof(Bar));

            IMongoCollections mongoCollections = new MongoCollections();
            mongoCollections.Add(mongoCollectionFoo);
            mongoCollections.Add(mongoCollectionBar);

            MongoDbContextData mongoDbContextData = new MongoDbContextData(
                _mongoDatabase.Client,
                _mongoDatabase,
                mongoCollections);

            // Act
            IMongoCollection<Foo> collection = mongoDbContextData.GetCollection<Foo>();

            // Assert
            Assert.Same(mongoCollectionFoo, collection);
        }

        [Fact]
        public void GetCollection_GetBarCollection_ReturnsRightCollection()
        {
            // Arrange
            IMongoCollection<Foo> mongoCollectionFoo =
                _mongoDatabase.GetCollection<Foo>(nameof(Foo));
            IMongoCollection<Bar> mongoCollectionBar =
                _mongoDatabase.GetCollection<Bar>(nameof(Bar));

            IMongoCollections mongoCollections = new MongoCollections();
            mongoCollections.Add(mongoCollectionFoo);
            mongoCollections.Add(mongoCollectionBar);

            MongoDbContextData mongoDbContextData = new MongoDbContextData(
                _mongoDatabase.Client,
                _mongoDatabase,
                mongoCollections);

            // Act
            IMongoCollection<Bar> collection = mongoDbContextData.GetCollection<Bar>();

            // Assert
            Assert.Same(mongoCollectionBar, collection);
        }

        [Fact]
        public void GetCollection_GetNotExistingCollection_ReturnsNewCollection()
        {
            // Arrange
            IMongoCollection<Foo> mongoCollectionFoo =
                _mongoDatabase.GetCollection<Foo>(nameof(Foo));
            IMongoCollection<Bar> mongoCollectionBar =
                _mongoDatabase.GetCollection<Bar>(nameof(Bar));

            IMongoCollections mongoCollections = new MongoCollections();
            mongoCollections.Add(mongoCollectionFoo);

            MongoDbContextData mongoDbContextData = new MongoDbContextData(
                _mongoDatabase.Client,
                _mongoDatabase,
                mongoCollections);

            // Act
            IMongoCollection<Bar> collection = mongoDbContextData.GetCollection<Bar>();

            // Assert
            Assert.NotNull(collection);
            Assert.NotSame(mongoCollectionBar, collection);
            Assert.Equal("Bar", collection.CollectionNamespace.CollectionName);
        }

        [Fact]
        public void GetCollection_GetNotExistingCollectionTwice_ReturnsSameCollection()
        {
            // Arrange
            IMongoCollections mongoCollections = new MongoCollections();
            
            MongoDbContextData mongoDbContextData = new MongoDbContextData(
                _mongoDatabase.Client,
                _mongoDatabase,
                mongoCollections);

            // Act
            IMongoCollection<Bar> collection1 = mongoDbContextData.GetCollection<Bar>();
            IMongoCollection<Bar> collection2 = mongoDbContextData.GetCollection<Bar>();

            // Assert
            Assert.NotNull(collection1);
            Assert.Same(collection1, collection2);
        }

        #endregion

        #region Get Client Tests

        [Fact]
        public void Client_GetClient_ReturnsRightClient()
        {
            // Arrange
            MongoDbContextData mongoDbContextData = new MongoDbContextData(
                _mongoDatabase.Client,
                _mongoDatabase,
                new MongoCollections());

            // Act
            IMongoClient client = mongoDbContextData.Client;

            // Assert
            Assert.Same(_mongoDatabase.Client, client);
        }

        #endregion

        #region Get Database Tests

        [Fact]
        public void Database_GetDatabase_ReturnsRightDatabase()
        {
            // Arrange
            MongoDbContextData mongoDbContextData = new MongoDbContextData(
                _mongoDatabase.Client,
                _mongoDatabase,
                new MongoCollections());

            // Act
            IMongoDatabase database = mongoDbContextData.Database;

            // Assert
            Assert.Same(_mongoDatabase, database);
        }

        #endregion
    }
}
