using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Squadron;
using Xunit;

namespace MongoDB.Extensions.Context.Overlap.Tests;

public class MongoDatabaseBuilderTests : IClassFixture<MongoResource>
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
    public void RegisterSerializer_OverwrightDefaultObjectSerializer_ObjectSerializerOverwritten()
    {
        // Arrange
        var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

        var newSerializer = new TestObjectSerializer();

        mongoDatabaseBuilder.RegisterSerializer(newSerializer);

        // Act
        mongoDatabaseBuilder.Build();

        // Assert
        IBsonSerializer<object> registeredSerializer =
            BsonSerializer.LookupSerializer<object>();

        Assert.True(registeredSerializer is TestObjectSerializer);
    }

    private class TestObjectSerializer : ObjectSerializer
    {
        public TestObjectSerializer() : base(type => true)
        {
        }
    }
}
