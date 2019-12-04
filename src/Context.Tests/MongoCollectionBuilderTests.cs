using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Extensions.Context.Tests.Helpers;
using Squadron;
using Xunit;

namespace MongoDB.Extensions.Context.Tests
{
    public class MongoCollectionBuilderTests : IClassFixture<MongoResource>
    {
        private readonly IMongoDatabase _mongoDatabase;

        public MongoCollectionBuilderTests(MongoResource mongoResource)
        {
            _mongoDatabase = mongoResource.CreateDatabase();
        }

        #region WithCollectionName Tests

        [Fact]
        public void WithCollectionName_CollectionNameSet_MongoCollectionNameIsSet()
        {
            // Arrange
            var mongoCollectionBuilder = new MongoCollectionBuilder<Order>(_mongoDatabase);

            // Act
            mongoCollectionBuilder.WithCollectionName("nameOnlyForTest");
            IMongoCollection<Order> result = mongoCollectionBuilder.Build();

            // Assert
            Assert.Equal("nameOnlyForTest", result.CollectionNamespace.CollectionName);
        }

        [Fact]
        public void WithCollectionName_CollectionNameNotSet_MongoCollectionNameIsDefault()
        {
            // Arrange
            var mongoCollectionBuilder = new MongoCollectionBuilder<Order>(_mongoDatabase);

            // Act
            IMongoCollection<Order> result = mongoCollectionBuilder.Build();

            // Assert
            Assert.Equal("Order", result.CollectionNamespace.CollectionName);
        }

        [Fact]
        public void WithCollectionName_CollectionNameNullOrEmpty_ThrowsException()
        {
            // Arrange
            var mongoCollectionBuilder = new MongoCollectionBuilder<Order>(_mongoDatabase);

            // Act
            Action setName = () => mongoCollectionBuilder.WithCollectionName(null);

            // Assert
            Assert.Throws<ArgumentException>(setName);
        }

        #endregion

        #region AddBsonClassMap Tests

        [Fact]
        public void AddBsonClassMap_AddNewBsonClassMapOnce_BsonClassMapIsRegistered()
        {
            // Arrange
            var mongoCollectionBuilder = new MongoCollectionBuilder<Order>(_mongoDatabase);

            // Act
            mongoCollectionBuilder.AddBsonClassMap<Order>(cm => cm.AutoMap());
            IMongoCollection<Order> result = mongoCollectionBuilder.Build();

            // Assert
            Assert.True(BsonClassMap.IsClassMapRegistered(typeof(Order)));
        }

        [Fact]
        public void AddBsonClassMap_AddNewBsonClassWithoutBuilding_BsonClassMapIsNotRegistered()
        {
            // Arrange
            var mongoCollectionBuilder = new MongoCollectionBuilder<Order>(_mongoDatabase);

            // Act
            mongoCollectionBuilder.AddBsonClassMap<ItemClassMapNotRegistered>(cm => cm.AutoMap());

            // Assert
            Assert.False(BsonClassMap.IsClassMapRegistered(typeof(ItemClassMapNotRegistered)));
        }

        [Fact]
        public void AddBsonClassMap_BsonClassMapNotAdded_BsonClassMapIsNotRegistered()
        {
            // Arrange
            var mongoCollectionBuilder = new MongoCollectionBuilder<Order>(_mongoDatabase);

            // Act
            IMongoCollection<Order> result = mongoCollectionBuilder.Build();

            // Assert
            Assert.False(BsonClassMap.IsClassMapRegistered(typeof(ItemClassMapNotRegistered)));
        }

        [Fact]
        public void AddBsonClassMap_AddNewBsonClassMapSeveralTimes_BsonClassMapIsRegisteredOnce()
        {
            // Arrange
            var mongoCollectionBuilder = new MongoCollectionBuilder<Order>(_mongoDatabase);

            // Act
            mongoCollectionBuilder
                .AddBsonClassMap<Order>(cm => cm.AutoMap())
                .AddBsonClassMap<Order>(cm => cm.MapIdMember(c => c.Id))
                .AddBsonClassMap<Order>(cm => cm.AutoMap());
            IMongoCollection<Order> result = mongoCollectionBuilder.Build();

            // Assert
            Assert.True(BsonClassMap.IsClassMapRegistered(typeof(Order)));
        }
        
        private class ItemClassMapNotRegistered
        {
            public int Id { get; set; }
        }

        #endregion

        #region WithMongoCollectionSettings Tests

        [Fact]
        public void WithMongoCollectionSettings_ChangeCollectionSettings_MongoCollectionSettingsChangedSuccessfully()
        {
            // Arrange
            var mongoCollectionBuilder = new MongoCollectionBuilder<Order>(_mongoDatabase);
            
            // Act
            mongoCollectionBuilder.WithCollectionSettings(mongoCollectionSettings =>
            {
                mongoCollectionSettings.WriteConcern = WriteConcern.Unacknowledged;
                mongoCollectionSettings.ReadConcern = ReadConcern.Linearizable;
                mongoCollectionSettings.ReadPreference = ReadPreference.Nearest;
                mongoCollectionSettings.AssignIdOnInsert = false;
            });
            IMongoCollection<Order> result = mongoCollectionBuilder.Build();

            // Assert
            Assert.Equal(WriteConcern.Unacknowledged, result.Settings.WriteConcern);
            Assert.Equal(ReadConcern.Linearizable, result.Settings.ReadConcern);
            Assert.Equal(ReadPreference.Nearest, result.Settings.ReadPreference);
            Assert.False(result.Settings.AssignIdOnInsert);
        }

        [Fact]
        public void WithMongoCollectionSettings_NoCollectionSettingsConfigured_DefaultMongoCollectionSettingsSet()
        {
            // Arrange
            var mongoCollectionBuilder = new MongoCollectionBuilder<Order>(_mongoDatabase);

            // Act
            IMongoCollection<Order> result = mongoCollectionBuilder.Build();

            // Assert
            Assert.Equal(WriteConcern.Acknowledged, result.Settings.WriteConcern);
            Assert.Equal(ReadConcern.Majority, result.Settings.ReadConcern);
            Assert.Equal(ReadPreference.Primary, result.Settings.ReadPreference);
            Assert.True(result.Settings.AssignIdOnInsert);
        }

        [Fact]
        public void WithMongoCollectionSettings_DefaultCollectionSettingsConfigured_DefaultMongoCollectionSettingsSet()
        {
            // Arrange
            var mongoCollectionBuilder = new MongoCollectionBuilder<Order>(_mongoDatabase);
            var mongoCollectionSettings = new MongoCollectionSettings();

            // Act
            mongoCollectionBuilder.WithCollectionSettings(mongoCollectionSettings => { });
            IMongoCollection <Order> result = mongoCollectionBuilder.Build();

            // Assert
            Assert.Equal(WriteConcern.Acknowledged, result.Settings.WriteConcern);
            Assert.Equal(ReadConcern.Majority, result.Settings.ReadConcern);
            Assert.Equal(ReadPreference.Primary, result.Settings.ReadPreference);
            Assert.True(result.Settings.AssignIdOnInsert);
        }

        #endregion

        #region WithMongoCollectionConfiguration Tests

        [Fact]
        public void WithMongoCollectionConfiguration_ChangeCollectionConfiguration_MongoCollectionConfigurationChangedSuccessfully()
        {
            // Arrange
            var mongoCollectionBuilder = new MongoCollectionBuilder<Order>(_mongoDatabase);

            // Act
            mongoCollectionBuilder.WithCollectionConfiguration(mongoCollection =>
            {
                mongoCollection.Indexes.CreateOne(new CreateIndexModel<Order>(
                    Builders<Order>.IndexKeys.Ascending(order => order.Name),
                    new CreateIndexOptions { Unique = true }));
            });

            IMongoCollection<Order> result = mongoCollectionBuilder.Build();

            // 
            List<BsonDocument> indexes = result.Indexes.List().ToList();
            Assert.Equal("_id_", indexes.First().GetElement("name").Value.ToString());
            Assert.Equal("Name_1", indexes.Last().GetElement("name").Value.ToString());
        }

        #endregion
    }
}
