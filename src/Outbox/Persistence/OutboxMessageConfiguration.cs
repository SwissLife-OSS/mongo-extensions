using MongoDB.Driver;
using MongoDB.Extensions.Context;
using MongoDB.Extensions.Outbox.Core;
using MongoDB.Extensions.Outbox.Persistence.Serialization;

namespace MongoDB.Extensions.Outbox.Persistence
{
    public class OutboxMessageConfiguration
        : IMongoCollectionConfiguration<OutboxMessage>
    {
        public void OnConfiguring(
            IMongoCollectionBuilder<OutboxMessage> mongoCollectionBuilder)
        {
            mongoCollectionBuilder
                .WithCollectionName("outbox.messages")
                .AddBsonClassMap<OutboxMessage>(
                cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                    cm.MapIdField(c => c.Id);
                    cm.MapProperty(y => y.Payload)
                        .SetSerializer(new DataObjectSerializer());
                })
                .WithCollectionConfiguration(settings =>
                {
                    settings
                        .WithReadPreference(ReadPreference.Nearest)
                        .WithReadConcern(ReadConcern.Majority)
                        .WithWriteConcern(WriteConcern.WMajority); //TODO: WithJournal?

                    CreateIndexes(settings.Indexes);
                });
        }

        private void CreateIndexes(
                IMongoIndexManager<OutboxMessage> indexes)
        {
            IndexKeysDefinition<OutboxMessage> indexDefinition
                = Builders<OutboxMessage>.IndexKeys.Descending(nameof(OutboxMessage.CreationDate));

            var indexModel = new CreateIndexModel<OutboxMessage>(indexDefinition);

            indexes.CreateOne(indexModel);
        }
    }
}
