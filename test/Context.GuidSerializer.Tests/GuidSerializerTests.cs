using MongoDB.Driver;
using Squadron;
using Xunit;
using System;
using System.Threading.Tasks;
using Snapshooter.Xunit;
using MongoDB.Prime.Extensions;

namespace MongoDB.Extensions.Context.GuidSerializers.Tests;

public class GuidSerializerTests : IClassFixture<MongoResource>
{
    private readonly MongoOptions _mongoOptions;
    private readonly IMongoDatabase _mongoDatabase;

    public GuidSerializerTests(MongoResource mongoResource)
    {
        _mongoDatabase = mongoResource.CreateDatabase();
        _mongoOptions = new MongoOptions
        {
            ConnectionString = mongoResource.ConnectionString,
            DatabaseName = _mongoDatabase.DatabaseNamespace.DatabaseName
        };
    }

    [Fact]
    public async Task Serialize_GuidPropertyGuidSerialized_Successfully()
    {
        // Arrange
        var foobarMongoDbContext = new FooBarMongoDbContext(_mongoOptions);

        IMongoCollection<Foo> collection =
            _mongoDatabase.GetCollection<Foo>("foos");

        Foo foo = new Foo
        (
            fooId: Guid.Parse("b1eba0d6-a1f9-4e31-bd70-0feed19f4492"),
            name: "test",
            additionalId: Guid.Parse("b58ec857-c874-457e-8662-133a055282f6")
        );

        // Act
        await collection.InsertOneAsync(foo);

        // Assert
        Snapshot.Match(collection.Dump());
    }

    [Fact]
    public async Task Serialize_ObjectPropertyGuidSerialized_Successfully()
    {
        // Arrange
        var foobarMongoDbContext = new FooBarMongoDbContext(_mongoOptions);

        IMongoCollection<Bar> collection =
            _mongoDatabase.GetCollection<Bar>("bars");

        Bar bar = new Bar
        (
            fooId: Guid.Parse("b1eba0d6-a1f9-4e31-bd70-0feed19f4492"),
            name: "test",
            additionalId: Guid.Parse("b58ec857-c874-457e-8662-133a055282f6")
        );

        // Act
        await collection.InsertOneAsync(bar);

        // Assert
        Snapshot.Match(collection.Dump());
    }
}
