using System;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Extensions.Context.Exceptions;
using MongoDB.Extensions.Context.Internal;
using Snapshooter.Xunit;
using Squadron;
using Xunit;
using Xunit.Priority;

namespace MongoDB.Extensions.Context.AllowedTypes.Tests;

[Collection(CollectionFixtureNames.MongoCollectionFixture)]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class MongoDatabaseBuilderTests
{
    private readonly MongoOptions _mongoOptions;
    private readonly IMongoDatabase _mongoDatabase;

    public MongoDatabaseBuilderTests(MongoResource mongoResource)
    {
        _mongoDatabase = mongoResource.CreateDatabase();
        _mongoOptions = new MongoOptions
        {
            ConnectionString = mongoResource.ConnectionString,
            DatabaseName = _mongoDatabase.DatabaseNamespace.DatabaseName
        };
    }

    [Fact]
    [Priority(0)]
    public void AddAllowedTypes_NoAllowedTypesRegisteredByDefault_ThrowsException()
    {
        // Arrange
        var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

        mongoDatabaseBuilder.ClearAllowedTypes();

        // Act
        Action action = () => mongoDatabaseBuilder.Build();

        // Assert
        Assert.Throws<MissingAllowedTypesException>(action);
    }

    [Fact]
    public void AddAllowedTypes_AddAllowedTypesOfAllDependencies_Success()
    {
        // Arrange
        var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

        mongoDatabaseBuilder.ClearAllowedTypes();

        // Act
        mongoDatabaseBuilder.AddAllowedTypesOfAllDependencies();
        mongoDatabaseBuilder.Build();

        // Assert
        IBsonSerializer<object> registeredSerializer =
            BsonSerializer.LookupSerializer<object>();

        Assert.True(registeredSerializer is TypeObjectSerializer);
        Snapshot.Match(new {
            TypeObjectSerializer.AllowedTypes,
            TypeObjectSerializer.AllowedTypesByNamespaces,
            TypeObjectSerializer.AllowedTypesByDependencies });
    }

    [Fact]
    public void AddAllowedTypes_AddAllowedTypesByNamespaces_Success()
    {
        // Arrange
        var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

        mongoDatabaseBuilder.ClearAllowedTypes();

        // Act
        mongoDatabaseBuilder.AddAllowedTypes("Mongo", "SwissLife");
        mongoDatabaseBuilder.Build();

        // Assert
        IBsonSerializer<object> registeredSerializer =
            BsonSerializer.LookupSerializer<object>();

        Assert.True(registeredSerializer is TypeObjectSerializer);
        Snapshot.Match(new {
            TypeObjectSerializer.AllowedTypes,
            TypeObjectSerializer.AllowedTypesByNamespaces,
            TypeObjectSerializer.AllowedTypesByDependencies });
    }

    [Fact]
    public void AddAllowedTypes_AddAllowedTypesByTypes_Success()
    {
        // Arrange
        var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

        mongoDatabaseBuilder.ClearAllowedTypes();

        // Act
        mongoDatabaseBuilder.AddAllowedTypes(typeof(Foo), typeof(Bar));
        mongoDatabaseBuilder.Build();

        // Assert
        IBsonSerializer<object> registeredSerializer =
            BsonSerializer.LookupSerializer<object>();

        Assert.True(registeredSerializer is TypeObjectSerializer);
        Snapshot.Match(new {
            TypeObjectSerializer.AllowedTypes,
            TypeObjectSerializer.AllowedTypesByNamespaces,
            TypeObjectSerializer.AllowedTypesByDependencies });
    }

    public class Bar
    {
        public int Id { get; set; }
        public string? BarName { get; set; }
    }

    public class Foo
    {
        public int Id { get; set; }
        public string? FooName { get; set; }
    }
}
