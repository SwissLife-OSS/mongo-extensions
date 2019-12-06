using System;
using Models;
using MongoDB.Driver;
using MongoDB.Extensions.Context;

namespace SimpleBlog.DataAccess
{
    internal class BlogCollectionConfiguration : IMongoCollectionConfiguration<Blog>
    {
        public void OnConfiguring(IMongoCollectionBuilder<Blog> mongoCollectionBuilder)
        {
            mongoCollectionBuilder
                .WithCollectionName("blogs")
                .AddBsonClassMap<Blog>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember<Guid>(c => c.Id);
                })
                .WithCollectionSettings(settings => settings.ReadConcern = ReadConcern.Majority)
                .WithCollectionSettings(settings => settings.ReadPreference = ReadPreference.Nearest)
                .WithCollectionConfiguration(collection =>
                {
                    var timestampIndex = new CreateIndexModel<Blog>(
                        Builders<Blog>.IndexKeys.Ascending(blog => blog.TimeStamp),
                        new CreateIndexOptions { Unique = false });

                    collection.Indexes.CreateOne(timestampIndex);
                });
        }
    }
}
