using System.Threading;
using System.Threading.Tasks;
using MongoDB.Extensions.Context;

namespace MongoDB.Extensions.Session.Abstractions;

public interface ISessionProvider<TContext>
    where TContext : IMongoDbContext
{
    Task<ITransactionSession> BeginTransactionAsync(
        CancellationToken cancellationToken);

    Task<ISession> StartSessionAsync(
        CancellationToken cancellationToken);
}
