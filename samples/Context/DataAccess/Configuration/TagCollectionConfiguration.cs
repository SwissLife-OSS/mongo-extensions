using Models;
using MongoDB.Driver;
using MongoDB.Extensions.Context;
using Tag = Models.Tag;

namespace SimpleBlog.DataAccess
{
    internal class TagCollectionConfiguration : IMongoCollectionConfiguration<Tag>
    {
        public void OnConfiguring(IMongoCollectionBuilder<Tag> mongoCollectionBuilder)
        {
            mongoCollectionBuilder
                .AddBsonClassMap<Tag>(cm => cm.AutoMap())
                .WithCollectionSettings(setting =>
                {
                    setting.ReadPreference = ReadPreference.Nearest;
                    setting.ReadConcern = ReadConcern.Available;
                    setting.WriteConcern = WriteConcern.Acknowledged;
                })
                .WithCollectionConfiguration(collection =>
                {
                    var timestampIndex = new CreateIndexModel<Tag>(
                        Builders<Tag>.IndexKeys.Ascending(tag => tag.Name),
                        new CreateIndexOptions { Unique = true });

                    collection.Indexes.CreateOne(timestampIndex);
                });
        }
    }
}