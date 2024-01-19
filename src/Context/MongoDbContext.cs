using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context;

public abstract class MongoDbContext : IMongoDbContext, IMongoDbTransaction
{
    private MongoDbContextData? _mongoDbContextData;

    private readonly object _lockObject = new object();

    public MongoDbContext(MongoOptions mongoOptions) : this(mongoOptions, true)
    {
    }

    public MongoDbContext(MongoOptions mongoOptions, bool enableAutoInitialize)
    {
        MongoOptions = mongoOptions.Validate();

        if (enableAutoInitialize)
        {
            Initialize();
        }
    }
    public MongoOptions MongoOptions { get; }

    public IMongoClient Client
    {
        get
        {
            EnsureInitialized();
            return _mongoDbContextData!.Client;
        }
    }

    public IMongoDatabase Database
    {
        get
        {
            EnsureInitialized();
            return _mongoDbContextData!.Database;
        }
    }
            
    public IMongoCollection<TDocument> CreateCollection<TDocument>()
            where TDocument : class
    {
        EnsureInitialized();
        return _mongoDbContextData!.GetCollection<TDocument>();
    }

    protected abstract void OnConfiguring(IMongoDatabaseBuilder mongoDatabaseBuilder);
    
    public virtual void Initialize()
    {
        if(_mongoDbContextData == null)
        {
            lock (_lockObject)
            {
                if (_mongoDbContextData == null)
                {
                    var mongoDatabaseBuilder = new MongoDatabaseBuilder(MongoOptions);

                    OnConfiguring(mongoDatabaseBuilder);

                    _mongoDbContextData = mongoDatabaseBuilder.Build();
                }
            }
        }
    }

    private void EnsureInitialized()
    {
        if (_mongoDbContextData == null)
        {
            lock (_lockObject)
            {
                if (_mongoDbContextData == null)
                {
                    throw new InvalidOperationException("MongoDbContext not initialized.");
                }
            }
        }
    }

    public async Task<IMongoTransactionDbContext> StartNewTransactionAsync(
        TransactionOptions? transactionOptions = null,
        CancellationToken cancellationToken = default)
    {
        transactionOptions ??= DefaultDefinitions.DefaultTransactionOptions;

        IClientSessionHandle clientSession = await Client
            .StartSessionAsync(cancellationToken: cancellationToken);

        clientSession.StartTransaction(transactionOptions);

        return new MongoTransactionDbContext(
            clientSession, transactionOptions, this);
    }
}
