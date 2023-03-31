using MongoDB.Extensions.Context;
using MongoDB.Extensions.Outbox.Core;

namespace MongoDB.Extensions.Outbox.Persistence
{
    public class OutboxDbContext : MongoDbContext, IOutboxDbContext
    {
        private readonly MessageOptions _messageOptions;

        public OutboxDbContext(
            MongoOptions mongoOptions,
            MessageOptions messageOptions)
            : base(mongoOptions, enableAutoInitialize: false)
        {
            _messageOptions = messageOptions;
        }

        protected override void OnConfiguring(
            IMongoDatabaseBuilder mongoDatabaseBuilder)
        {
            mongoDatabaseBuilder
                .ConfigureCollection(new OutboxMessageConfiguration())
                .ConfigureCollection(new MessageLockConfiguration(_messageOptions.LockDuration))
                .AddAllowedTypes("MongoDB.Extensions.Outbox");
        }
    }
}
