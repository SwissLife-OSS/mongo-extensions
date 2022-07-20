using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Extensions.Migration;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Migration.Tests;
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
        IMongoDatabase database = resource.Client.GetDatabase("Scenario1-up");
        _typedCollection = database.GetCollection<TestEntityForUp>("TestEntityForUp");
        _untypedCollection = database.GetCollection<BsonDocument>("TestEntityForUp");
    }

    static void RegisterMongoMigrations()
    {
        MigrationOption options = new MigrationOptionBuilder()
            .ForEntity<TestEntityForUp>(o => o
                .WithMigration(new TestMigration1())
                .WithMigration(new TestMigration2())
                .WithMigration(new TestMigration3()))
            .Build();
        var context = new MigrationContext(options, NullLoggerFactory.Instance);

        BsonSerializer.RegisterSerializationProvider(new MigrationSerializerProvider(context));
    }

    [Fact]
    public async Task Scenario1_AddRetrieve_NoMigration()
    {
        // Arrange
        const string input = "Bar";
        await _typedCollection.InsertOneAsync(new TestEntityForUp("1", input));

        // Act
        var result = await _typedCollection.AsQueryable().SingleOrDefaultAsync(c => c.Id == "1");

        // Assert
        result.Foo.Should().Be(input);
    }

    [Fact]
    public async Task Scenario1_RetrieveWithoutVersion_MigratedToNewestVersion()
    {
        // Arrange
        await _untypedCollection.InsertOneAsync(new BsonDocument(new Dictionary<string, object>
            { ["_id"] = "2", ["Foo"] = "Bar" }));

        // Act
        TestEntityForUp result = await _typedCollection.AsQueryable().SingleOrDefaultAsync(c => c.Id == "2");

        // Assert
        result.Foo.Should().Be("Bar Migrated Up to 1 Migrated Up to 2 Migrated Up to 3");
    }

    [Fact]
    public async Task Scenario1_RetrieveAtNewUnknownVersion_NoMigration()
    {
        // Arrange
        await _untypedCollection.InsertOneAsync(new BsonDocument(new Dictionary<string, object>
            { ["_id"] = "3", ["Foo"] = "Bar", ["Version"] = 4 }));

        // Act
        TestEntityForUp result = await _typedCollection.AsQueryable().SingleOrDefaultAsync(c => c.Id == "3");

        // Assert
        result.Foo.Should().Be("Bar");
    }

    [Fact]
    public async Task Scenario1_RetrieveAtVersion2_MigratedToVersion3()
    {
        // Arrange
        await _untypedCollection.InsertOneAsync(new BsonDocument(new Dictionary<string, object>
        { ["_id"] = "4", ["Foo"] = "Bar", ["Version"] = 2 }));

        // Act
        TestEntityForUp result = await _typedCollection.AsQueryable().SingleOrDefaultAsync(c => c.Id == "4");

        // Assert
        result.Foo.Should().Be("Bar Migrated Up to 3");
    }
}
