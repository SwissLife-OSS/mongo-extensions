using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Extensions.Outbox.Core
{
    public interface ISessionProvider
    {
        Task<ITransactionSession> BeginTransactionAsync(
            CancellationToken cancellationToken);
    }
}
