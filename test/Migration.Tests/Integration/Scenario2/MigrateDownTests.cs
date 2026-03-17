using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Extensions.Migration;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Migration.Tests;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Squadron;
using Xunit;

namespace MongoMigrationTest.Integration.Scenario2;

[Collection("SharedMongoDbCollection")]
public class MigrateDownTests
{
    readonly IMongoCollection<TestEntityForDown> _typedCollection;
    readonly IMongoCollection<BsonDocument> _untypedCollection;

    public MigrateDownTests(MongoResource resource)
    {
        RegisterMongoMigrations();
        IMongoDatabase database = resource.Client.GetDatabase("Scenario2-down");
        _typedCollection = database.GetCollection<TestEntityForDown>("TestEntityForDown");
        _untypedCollection = database.GetCollection<BsonDocument>("TestEntityForDown");
    }

    static void RegisterMongoMigrations()
    {
        MigrationOption options = new MigrationOptionBuilder()
            .ForEntity<TestEntityForDown>(o => o
                .WithMigration(new TestMigration1())
                .WithMigration(new TestMigration2())
                .WithMigration(new TestMigration3())
                .AtVersion(2))
            .Build();
        var context = new MigrationContext(options, NullLoggerFactory.Instance);

        BsonSerializer.RegisterSerializationProvider(new MigrationSerializerProvider(context));
    }

    [Fact]
    public async Task Scenario2_RetrieveAtNewUnknownVersion_MigrateDownTo2()
    {
        // Arrange
        await _untypedCollection.InsertOneAsync(new BsonDocument(new Dictionary<string, object>
            { ["_id"] = "3", ["Foo"] = "Bar", ["Version"] = 4 }));

        // Act
        TestEntityForDown result = await _typedCollection.AsQueryable().SingleOrDefaultAsync(c => c.Id == "3");

        // Assert
        result.Foo.Should().Be("Bar Migrated Down to 2");
    }

    [Fact]
    public async Task Scenario2_RetrieveAtVersion3_MigratedToVersion2()
    {
        // Arrange
        await _untypedCollection.InsertOneAsync(new BsonDocument(new Dictionary<string, object>
            { ["_id"] = "4", ["Foo"] = "Bar", ["Version"] = 3 }));

        // Act
        TestEntityForDown result = await _typedCollection.AsQueryable().SingleOrDefaultAsync(c => c.Id == "4");

        // Assert
        result.Foo.Should().Be("Bar Migrated Down to 2");
    }
}
