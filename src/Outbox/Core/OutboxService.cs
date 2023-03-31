using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Extensions.Outbox.Core
{
    public class OutboxService : IOutboxService
    {
        private readonly IOutboxRepository _outboxRepository;

        public OutboxService(
            IOutboxRepository outboxRepository)
        {
            _outboxRepository = outboxRepository;
        }

        public Task AddMessagesAsync(
            ITransactionSession session,
            IReadOnlyList<object> payloads,
            CancellationToken token)
        {
            IReadOnlyList<OutboxMessage> messages = payloads
                .Select(p => new OutboxMessage(
                    id: Guid.NewGuid(),
                    payload: p,
                    creationDate: DateTime.UtcNow))
                .ToArray();

            return _outboxRepository.AddMessagesAsync(session, messages, token);
        }
    }
}
