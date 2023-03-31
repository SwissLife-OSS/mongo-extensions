using System.Threading;
using System.Threading.Tasks;

namespace SwissLife.MongoDB.Extensions.Outbox.Core
{
    public interface ISessionProvider
    {
        Task<ITransactionSession> BeginTransactionAsync(
            CancellationToken cancellationToken);
    }
}
