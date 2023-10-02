using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Extensions.Session;

public interface ISessionProvider<TScope>
{
    Task<ITransactionSession> BeginTransactionAsync(
        CancellationToken cancellationToken);

    Task<ISession> StartSessionAsync(
        CancellationToken cancellationToken);
}
