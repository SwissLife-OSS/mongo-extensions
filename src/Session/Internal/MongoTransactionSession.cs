using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoDB.Extensions.Session;

internal class MongoTransactionSession : ITransactionSession
{
    private readonly CancellationToken _cancellationToken;
    private bool _disposed;

    public MongoTransactionSession(
        IClientSessionHandle clientSession,
        CancellationToken cancellationToken)
    {
        Session = clientSession;
        _cancellationToken = cancellationToken;
    }

    public IClientSessionHandle Session { get; }

    public async Task CommitAsync()
    {
        await Session.CommitTransactionAsync(_cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Session.Dispose();
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
