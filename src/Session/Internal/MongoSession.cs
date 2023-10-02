using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoDB.Extensions.Session;

internal sealed class MongoSession : ISession
{
    private readonly IClientSessionHandle _session;
    private bool _disposed;

    private static TransactionOptions TransactionOptions { get; } = new(
        ReadConcern.Majority,
        ReadPreference.Primary,
        WriteConcern.WMajority.With(journal: true),
        TimeSpan.FromSeconds(180));

    public MongoSession(IClientSessionHandle clientSession)
    {
        _session = clientSession;
    }

    public Task<T> WithTransactionAsync<T>(
        Func<ISession, CancellationToken, Task<T>> action,
        CancellationToken cancellationToken)
    {
        return _session.WithTransactionAsync<T>(
            (_, ct) => action(this, ct),
            TransactionOptions,
            cancellationToken);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _session.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
