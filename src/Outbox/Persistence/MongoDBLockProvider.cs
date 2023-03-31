using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Extensions.Outbox.Core;

namespace MongoDB.Extensions.Outbox.Persistence
{
    public class MongoDBLockProvider : ILockProvider
    {
        private readonly IMongoCollection<MessageLock> _messagesLocks;

        public MongoDBLockProvider(
            IOutboxDbContext dbContext)
        {
            _messagesLocks = dbContext.CreateCollection<MessageLock>();
        }

        public async Task<bool> AcquireMessageLockAsync(
            Guid messageId, CancellationToken token)
        {
            FilterDefinition<MessageLock> filter
                = Builders<MessageLock>.Filter.Eq(l => l.Id, messageId);

            UpdateDefinition<MessageLock> setOnInsert =
            Builders<MessageLock>.Update.SetOnInsert(
                    item => item.LockedAt, DateTime.UtcNow);

            UpdateResult operationResult = await _messagesLocks
                .UpdateOneAsync(
                    filter,
                    setOnInsert,
                    options: new UpdateOptions
                    {
                        IsUpsert = true,
                    },
                    cancellationToken: token);

            if (operationResult.IsAcknowledged
                && operationResult.UpsertedId != null)
            {
                // we got the lock
                return true;
            }

            //we didn't get the lock
            return false;
        }
    }
}
