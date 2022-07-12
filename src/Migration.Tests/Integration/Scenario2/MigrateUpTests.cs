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

namespace MongoMigrationTest.Integration.Scenario2;

[Collection("SharedMongoDbCollection")]
public class MigrateUpTests
{
    readonly IMongoCollection<TestEntityForUp> _typedCollection;
    readonly IMongoCollection<BsonDocument> _untypedCollection;

    public MigrateUpTests(MongoResource resource)
    {
        RegisterMongoMigrations();
        IMongoDatabase database = resource.Client.GetDatabase("Scenario2-up");
        _typedCollection = database.GetCollection<TestEntityForUp>("TestEntityForUp");
        _untypedCollection = database.GetCollection<BsonDocument>("TestEntityForUp");
    }

    static void RegisterMongoMigrations()
    {
        MigrationOption options = new MigrationOptionBuilder()
            .ForEntity<TestEntityForUp>(o => o
                .WithMigration(new TestMigration1())
                .WithMigration(new TestMigration2())
                .WithMigration(new TestMigration3())
                .AtVersion(2))
            .Build();
        var context = new MigrationContext(options, NullLoggerFactory.Instance);

        BsonSerializer.RegisterSerializationProvider(new MigrationSerializerProvider(context));
    }

    [Fact]
    public async Task Scenario2_AddRetrieve_NoMigration()
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
    public async Task Scenario2_RetrieveWithoutVersion_MigratedToCurrentVersion()
    {
        // Arrange
        await _untypedCollection.InsertOneAsync(new BsonDocument(new Dictionary<string, object>
        { ["_id"] = "2", ["Foo"] = "Bar" }));

        // Act
        TestEntityForUp result = await _typedCollection.AsQueryable().SingleOrDefaultAsync(c => c.Id == "2");

        // Assert
        result.Foo.Should().Be("Bar Migrated Up to 1 Migrated Up to 2");
    }
}
