using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Extensions.Outbox.Core
{
    public interface IOutboxRepository
    {
        Task<ITransactionSession> BeginTransactionAsync(CancellationToken token);

        Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(
            CancellationToken token);

        Task DeleteMessageAsync(
            ITransactionSession session,
            Guid messageId,
            CancellationToken token);

        Task AddMessagesAsync(
            ITransactionSession session,
            IReadOnlyList<OutboxMessage> messages,
            CancellationToken token);
    }
}
