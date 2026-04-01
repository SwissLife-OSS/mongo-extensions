using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoDB.Extensions.Session;

public interface ISessionFactory
{
    ValueTask<IClientSessionHandle> CreateSessionAsync(
        CancellationToken cancellationToken);
}
