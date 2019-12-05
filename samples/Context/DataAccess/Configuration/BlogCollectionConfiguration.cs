using Models;
using MongoDB.Driver;
using MongoDB.Extensions.Context;

namespace DataAccess
{
    internal class BlogCollectionConfiguration : IMongoCollectionConfiguration<Blog>
    {
        public void Configure(IMongoCollectionBuilder<Blog> mongoCollectionBuilder)
        {
            mongoCollectionBuilder
                .WithCollectionName("blogs")
                .AddBsonClassMap<Blog>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember<string>(c => c.Id);
                })
                .WithMongoCollectionSettings(settings => settings.ReadConcern = ReadConcern.Majority)
                .WithMongoCollectionSettings(settings => settings.ReadPreference = ReadPreference.Nearest)
                .WithMongoCollectionConfiguration(collection =>
                {
                    var timestampIndex = new CreateIndexModel<Blog>(
                        Builders<Blog>.IndexKeys.Ascending(blog => blog.TimeStamp),
                        new CreateIndexOptions { Unique = false });

                    collection.Indexes.CreateOne(timestampIndex);
                });
        }
    }
}
