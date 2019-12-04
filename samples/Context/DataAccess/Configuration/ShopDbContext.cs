using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Extensions.Context;

namespace DataAccess
{
    public class ShopDbContext : MongoDbContext
    {
        public ShopDbContext(MongoOptions mongoOptions) : base(mongoOptions)
        {
        }

        protected override void OnConfiguring(IMongoDatabaseBuilder mongoDatabaseBuilder)
        {
            mongoDatabaseBuilder
                .RegisterCamelCaseConventionPack()
                .RegisterSerializer<DateTimeOffset>(new DateTimeOffsetSerializer(BsonType.String))
                .RegisterSerializer<string>(new StringSerializer(BsonType.String))
                .ConfigureConnection(setting => setting.WriteConcern = WriteConcern.WMajority)
                .ConfigureConnection(setting =>
                {
                    setting.ReadConcern = ReadConcern.Available;
                    setting.ReadPreference = ReadPreference.Primary;
                })
                .ConfigureCollection(new ProductCollectionConfiguration());
        }
    }
}
