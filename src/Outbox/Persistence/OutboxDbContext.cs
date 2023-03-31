using MongoDB.Extensions.Context;
using SwissLife.MongoDB.Extensions.Outbox.Core;

namespace SwissLife.MongoDB.Extensions.Outbox.Persistence
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
                .ConfigureCollection(new MessageLockConfiguration(_messageOptions.LockDuration));
        }
    }
}
