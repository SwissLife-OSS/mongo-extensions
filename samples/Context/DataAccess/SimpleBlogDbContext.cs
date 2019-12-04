using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Extensions.Context;

namespace DataAccess
{
    public class SimpleBlogDbContext : MongoDbContext
    {
        public SimpleBlogDbContext(MongoOptions mongoOptions) : base(mongoOptions)
        {
        }

        protected override void OnConfiguring(IMongoDatabaseBuilder mongoDatabaseBuilder)
        {
            mongoDatabaseBuilder
                .RegisterCamelCaseConventionPack()
                .RegisterSerializer(new DateTimeOffsetSerializer())
                .ConfigureConnection(con => con.ReadConcern = ReadConcern.Majority)
                .ConfigureConnection(con => con.WriteConcern = WriteConcern.WMajority)
                .ConfigureConnection(con => con.ReadPreference = ReadPreference.Primary)
                .ConfigureCollection(new UserCollectionConfiguration())
                .ConfigureCollection(new BlogCollectionConfiguration())
                .ConfigureCollection(new TagCollectionConfiguration());
        }
    }
}
