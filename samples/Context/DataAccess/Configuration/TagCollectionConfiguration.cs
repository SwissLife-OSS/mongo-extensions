using Models;
using MongoDB.Driver;
using MongoDB.Extensions.Context;
using Tag = Models.Tag;

namespace DataAccess
{
    internal class TagCollectionConfiguration : IMongoCollectionConfiguration<Tag>
    {
        public void Configure(IMongoCollectionBuilder<Tag> mongoCollectionBuilder)
        {
            mongoCollectionBuilder
                .AddBsonClassMap<Tag>(cm => cm.AutoMap())
                .WithMongoCollectionSettings(setting =>
                {
                    setting.ReadPreference = ReadPreference.Nearest;
                    setting.ReadConcern = ReadConcern.Available;
                    setting.WriteConcern = WriteConcern.Acknowledged;
                })
                .WithMongoCollectionConfiguration(collection =>
                {
                    var timestampIndex = new CreateIndexModel<Tag>(
                        Builders<Tag>.IndexKeys.Ascending(tag => tag.Name),
                        new CreateIndexOptions { Unique = false });

                    collection.Indexes.CreateOne(timestampIndex);
                });
        }
    }
}