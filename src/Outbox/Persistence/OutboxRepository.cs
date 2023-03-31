using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Extensions.Outbox.Core;

namespace MongoDB.Extensions.Outbox.Persistence
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly OutboxOptions _options;
        private readonly IMongoCollection<OutboxMessage> _messages;
        private readonly ISessionProvider _sessionProvider;

        public OutboxRepository(
            OutboxOptions options,
            IOutboxDbContext dbContext,
            ISessionProvider sessionProvider)
        {
            _options = options;
            _messages = dbContext.CreateCollection<OutboxMessage>();
            _sessionProvider = sessionProvider;
        }

        public async Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(
            CancellationToken token)
        {
            DateTime maxCreationDate = DateTime.UtcNow.Add(
                -_options.FallbackWorker.MessageMinimumAge);

            FilterDefinition<OutboxMessage> filter = Builders<OutboxMessage>.Filter
                .Lt(m => m.CreationDate, maxCreationDate);

            IReadOnlyList<OutboxMessage> results
                = await _messages.Find(filter).ToListAsync(token);

            return results;
        }

        public Task<ITransactionSession> BeginTransactionAsync(CancellationToken token)
        {
            return _sessionProvider.BeginTransactionAsync(token);
        }

        public async Task DeleteMessageAsync(
            ITransactionSession session,
            Guid messageId,
            CancellationToken token)
        {
            await _messages
                .DeleteOneAsync(
                    session.GetSessionHandle(),
                    Builders<OutboxMessage>.Filter.Eq(l => l.Id, messageId),
                    cancellationToken: token);
        }

        public async Task AddMessagesAsync(
            ITransactionSession session,
            IReadOnlyList<OutboxMessage> messages,
            CancellationToken token)
        {
            await _messages.InsertManyAsync(
                session.GetSessionHandle(),
                messages,
                cancellationToken: token);
        }
    }
}
