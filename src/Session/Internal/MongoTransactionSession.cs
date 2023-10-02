using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoDB.Extensions.Session;

internal class MongoTransactionSession : ITransactionSession
{
    private readonly IClientSessionHandle _session;
    private readonly CancellationToken _cancellationToken;
    private bool _disposed;

    public MongoTransactionSession(
        IClientSessionHandle clientSession,
        CancellationToken cancellationToken)
    {
        _session = clientSession;
        _cancellationToken = cancellationToken;
    }

    public async Task CommitAsync()
    {
        await _session.CommitTransactionAsync(_cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
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
