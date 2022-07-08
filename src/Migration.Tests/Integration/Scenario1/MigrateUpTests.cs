using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Extensions.Migration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Xunit;
using MongoDB.Driver.Linq;
using Squadron;

namespace MongoMigrationTest.Integration.Scenario1;

[Collection("SharedMongoDbCollection")]
public class MigrateUpTests
{
    readonly IMongoCollection<TestEntityForUp> _typedCollection;
    readonly IMongoCollection<BsonDocument> _untypedCollection;

    public MigrateUpTests(MongoResource resource)
    {
        RegisterMongoMigrations();
        var database = resource.Client.GetDatabase("Scenario1-up");
        _typedCollection = database.GetCollection<TestEntityForUp>("TestEntityForUp");
        _untypedCollection = database.GetCollection<BsonDocument>("TestEntityForUp");
    }

    static void RegisterMongoMigrations()
    {
        var loggerFactory = LoggerFactory.Create(_ => { });

        var options = new MigrationOptionBuilder().ForEntity<TestEntityForUp>(o => o
                .WithMigration(new TestMigration1())
                .WithMigration(new TestMigration2())
                .WithMigration(new TestMigration3()))
            .Build();
        var context = new MigrationContext(options, loggerFactory);

        BsonSerializer.RegisterSerializationProvider(new MigrationSerializerProvider(context));
    }

    [Fact]
    public async Task Scenario1_AddRetrieve_NoMigration()
    {
        // Arrange
        const string input = "Bar";
        await _typedCollection.InsertOneAsync(new TestEntityForUp("1", input, 1));

        // Act
        var result = await _typedCollection.AsQueryable().SingleOrDefaultAsync(c => c.Id == "1");

        // Assert
        result.Foo.Should().Be(input);
    }

    [Fact]
    public async Task Scenario1_RetrieveAtVersion0_MigratedToNewestVersion()
    {
        // Arrange
        await _untypedCollection.InsertOneAsync(new BsonDocument(new Dictionary<string, object>
        { ["_id"] = "id0", ["Foo"] = "Bar", ["Version"] = 0 }));

        // Act
        TestEntityForUp result = await _typedCollection.AsQueryable().SingleOrDefaultAsync(c => c.Id == "id0");

        // Assert
        result.Foo.Should().Be("Bar Migrated Up to 1 Migrated Up to 2 Migrated Up to 3");
    }

    [Fact]
    public async Task Scenario1_RetrieveAtVersion2_MigratedToVersion3()
    {
        // Arrange
        await _untypedCollection.InsertOneAsync(new BsonDocument(new Dictionary<string, object>
        { ["_id"] = "id1", ["Foo"] = "Bar", ["Version"] = 2 }));

        // Act
        TestEntityForUp result = await _typedCollection.AsQueryable().SingleOrDefaultAsync(c => c.Id == "id1");

        // Assert
        result.Foo.Should().Be("Bar Migrated Up to 3");
    }
}