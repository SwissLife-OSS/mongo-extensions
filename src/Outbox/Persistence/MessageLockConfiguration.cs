using System;
using MongoDB.Driver;
using MongoDB.Extensions.Context;

namespace SwissLife.MongoDB.Extensions.Outbox.Persistence
{
    public class MessageLockConfiguration
        : IMongoCollectionConfiguration<MessageLock>
    {
        private readonly TimeSpan _lockTimeToLive;

        public MessageLockConfiguration(TimeSpan lockTimeToLive)
        {
            _lockTimeToLive = lockTimeToLive;
        }

        public void OnConfiguring(
            IMongoCollectionBuilder<MessageLock> mongoCollectionBuilder)
        {
            mongoCollectionBuilder
                .WithCollectionName("outbox.locks")
                .AddBsonClassMap<MessageLock>(
                cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                    cm.MapIdField(c => c.Id);
                })
                .WithCollectionConfiguration(setting =>
                {
                    setting
                        .WithReadPreference(ReadPreference.Primary)
                        .WithReadConcern(ReadConcern.Majority)
                        .WithWriteConcern(WriteConcern.WMajority);

                    CreateIndexes(setting.Indexes);
                });
        }

        private void CreateIndexes(
                IMongoIndexManager<MessageLock> indexes)
        {
            IndexKeysDefinition<MessageLock> indexDefinition
                = Builders<MessageLock>.IndexKeys.Ascending(nameof(MessageLock.LockedAt));

            var indexOptions = new CreateIndexOptions { ExpireAfter = _lockTimeToLive };

            var indexModel = new CreateIndexModel<MessageLock>(indexDefinition, indexOptions);

            indexes.CreateOne(indexModel);
        }
    }
}
