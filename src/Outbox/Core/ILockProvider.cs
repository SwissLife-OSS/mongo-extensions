using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Extensions.Outbox.Core
{
    public interface ILockProvider
    {
        Task<bool> AcquireMessageLockAsync(
            Guid messageId, CancellationToken token);
    }
}
