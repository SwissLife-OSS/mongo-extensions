using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Extensions.Context.GuidSerializers.Tests;

public class FooBarMongoDbContext : MongoDbContext
{
    public FooBarMongoDbContext(MongoOptions mongoOptions)
        : base(mongoOptions)
    {
    }

    protected override void OnConfiguring(
        IMongoDatabaseBuilder databaseBuilder)
    {
        databaseBuilder.RegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));
    }
}
