using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoDB.Extensions.Session;

internal sealed class MongoSession : ISession
{
    private readonly TransactionOptions _transactionOptions;
    private bool _disposed;

    public MongoSession(
        IClientSessionHandle clientSession,
        TransactionOptions transactionOptions)
    {
        _transactionOptions = transactionOptions;
        Session = clientSession;
    }

    public IClientSessionHandle Session { get; }

    public Task<T> WithTransactionAsync<T>(
        Func<ISession, CancellationToken, Task<T>> action,
        CancellationToken cancellationToken)
    {
        return Session.WithTransactionAsync<T>(
            (_, ct) => action(this, ct), _transactionOptions, cancellationToken);
    }

    public ITransactionSession StartTransaction(
        CancellationToken cancellationToken)
    {
        Session.StartTransaction(_transactionOptions);

        return new MongoTransactionSession(Session, cancellationToken);
    }

    private void Dispose(bool disposing)
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
