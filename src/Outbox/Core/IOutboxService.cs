using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SwissLife.MongoDB.Extensions.Outbox.Core
{
    public interface IOutboxService
    {
        Task AddMessagesAsync(
            ITransactionSession session,
            IReadOnlyList<object> payloads,
            CancellationToken token);
    }
}
