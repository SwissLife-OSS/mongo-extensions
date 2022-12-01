using System;
using System.Threading.Tasks;
using System.Threading;
using MongoDB.Driver;
using System.Collections.Concurrent;
using MongoDB.Extensions.Transactions;
using MongoDB.Extensions.Context.Internal;

namespace MongoDB.Extensions.Context;

public class MongoTransactionDbContext : IMongoTransactionDbContext
{
    private readonly MongoDbContext _mongoDbContext;
    private readonly MongoCollections _mongoCollections;
    private readonly object _lockObject = new object();

    public MongoTransactionDbContext(
        IClientSessionHandle clientSession,
        TransactionOptions transactionOptions,
        MongoDbContext mongoDbContext)
    {
        _mongoDbContext = mongoDbContext;
        _mongoCollections = new MongoCollections();

        ClientSession = clientSession;
        TransactionOptions = transactionOptions;
        MongoOptions = mongoDbContext.MongoOptions;        
        Client = mongoDbContext.Client.AsTransactionClient(clientSession);
        Database = mongoDbContext.Database.AsTransactionDatabase(clientSession);
    }

    public TransactionOptions TransactionOptions { get; }

    public MongoOptions MongoOptions { get; }

    public IMongoClient Client { get; }

    public IMongoDatabase Database { get; }

    public IClientSessionHandle ClientSession { get; }

    public IMongoCollection<TDocument> GetCollection<TDocument>()
        where TDocument : class => CreateCollection<TDocument>();

    public IMongoCollection<TDocument> CreateCollection<TDocument>()
        where TDocument : class
    {
        IMongoCollection<TDocument>? collection =
            _mongoCollections.TryGetCollection<TDocument>();

        if (collection is { })
        {
            return collection;
        }

        lock(_lockObject)
        {
            collection = _mongoCollections
                .TryGetCollection<TDocument>();

            if (collection is { })
            {
                return collection;
            }

            collection = _mongoDbContext
                .CreateCollection<TDocument>()
                .AsTransactionCollection(ClientSession);

            _mongoCollections.Add<TDocument>(collection);            
        }

        return collection;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await ClientSession
            .CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await ClientSession
            .AbortTransactionAsync(cancellationToken);
    }

    #region IDisposable

    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                ClientSession.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
