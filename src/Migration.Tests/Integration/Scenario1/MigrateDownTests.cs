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

namespace MongoMigrationTest.Integration.Scenario1;

[Collection("SharedMongoDbCollection")]
public class MigrateDownTests
{
    readonly IMongoCollection<TestEntityForDown> _typedCollection;
    readonly IMongoCollection<BsonDocument> _untypedCollection;

    public MigrateDownTests(MongoResource resource)
    {
        RegisterMongoMigrations();
        IMongoDatabase database = resource.Client.GetDatabase("Scenario1-down");
        _typedCollection = database.GetCollection<TestEntityForDown>("TestEntityForDown");
        _untypedCollection = database.GetCollection<BsonDocument>("TestEntityForDown");
    }

    static void RegisterMongoMigrations()
    {
        MigrationOption options = new MigrationOptionBuilder()
            .ForEntity<TestEntityForDown>(o => o.AtVersion(0)
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
        await _typedCollection.InsertOneAsync(new TestEntityForDown("1", input));

        // Act
        TestEntityForDown result = await _typedCollection.AsQueryable()
            .SingleOrDefaultAsync(c => c.Id == "1");

        // Assert
        result.Foo.Should().Be(input);
    }

    [Fact]
    public async Task Scenario1_RetrieveAtVersion3_MigratedDownTo0()
    {
        // Arrange
        await _untypedCollection.InsertOneAsync(new BsonDocument(new Dictionary<string, object>
        { ["_id"] = "id0", ["Foo"] = "Bar", ["Version"] = 3 }));

        // Act
        TestEntityForDown result = await _typedCollection.AsQueryable()
            .SingleOrDefaultAsync(c => c.Id == "id0");

        // Assert
        result.Foo.Should().Be("Bar Migrated Down to 2 Migrated Down to 1 Migrated Down to 0");
    }

    [Fact]
    public async Task Scenario1_RetrieveAtVersion2_MigratedToVersion3()
    {
        // Arrange
        await _untypedCollection.InsertOneAsync(new BsonDocument(new Dictionary<string, object>
        { ["_id"] = "id1", ["Foo"] = "Bar", ["Version"] = 1 }));

        // Act
        TestEntityForDown result = await _typedCollection.AsQueryable()
            .SingleOrDefaultAsync(c => c.Id == "id1");

        // Assert
        result.Foo.Should().Be("Bar Migrated Down to 0");
    }

}
