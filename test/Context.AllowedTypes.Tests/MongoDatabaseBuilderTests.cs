using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Extensions.Context.AllowedTypes.Tests.Helpers;
using Snapshooter.Xunit;
using Squadron;
using Xunit;

namespace MongoDB.Extensions.Context.AllowedTypes.Tests;

[Collection(CollectionFixtureNames.MongoCollectionFixture)]
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
    public void AddAllowedTypes_AllowedTypesRegisteredByDefault_Success()
    {
        // Arrange
        var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

        mongoDatabaseBuilder.ClearAllowedTypes();

        // Act
        MongoDbContextData context = mongoDatabaseBuilder.Build();

        // Assert
        Assert.NotNull(context);
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

        Assert.True(registeredSerializer is ObjectSerializer);
        Snapshot.Match(TestHelpers.GetTypeObjectSerializerContent(),
            options => options.Assert(fieldOption =>
                Assert.Contains("MongoDB", fieldOption
                    .Fields<string>("AllowedTypesByDependencies[*]"))));
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

        Assert.True(registeredSerializer is ObjectSerializer);
        Snapshot.Match(TestHelpers.GetTypeObjectSerializerContent());
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

        Assert.True(registeredSerializer is ObjectSerializer);
        Snapshot.Match(TestHelpers.GetTypeObjectSerializerContent());
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
