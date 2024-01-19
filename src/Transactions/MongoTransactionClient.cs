using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;

namespace MongoDB.Extensions.Transactions;

public class MongoTransactionClient : IMongoClient
{
    private readonly IMongoClient _client;
    private readonly IClientSessionHandle? _clientSessionHandle;

    public MongoTransactionClient(IMongoClient client)
    {
        _client = client;
    }

    public MongoTransactionClient(
        IMongoClient client,
        IClientSessionHandle clientSessionHandle)
    {
        _client = client;
        _clientSessionHandle = clientSessionHandle;
    }

    public void DropDatabase(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            _client.DropDatabase(session, name, cancellationToken);
            return;
        }

        _client.DropDatabase(name, cancellationToken);
    }

    public void DropDatabase(
        IClientSessionHandle session,
        string name,
        CancellationToken cancellationToken = default)
    {
        _client.DropDatabase(session, name, cancellationToken);
    }

    public Task DropDatabaseAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _client.DropDatabaseAsync(session, name, cancellationToken);
        }

        return _client.DropDatabaseAsync(name, cancellationToken);
    }

    public Task DropDatabaseAsync(
        IClientSessionHandle session,
        string name,
        CancellationToken cancellationToken = default)
    {
        return _client.DropDatabaseAsync(session, name, cancellationToken);
    }

    public IMongoDatabase GetDatabase(string name, MongoDatabaseSettings? settings = null)
    {
        return _client.GetDatabase(name, settings).AsTransactionDatabase();
    }

    public IAsyncCursor<string> ListDatabaseNames(CancellationToken cancellationToken = default)
    {
        return _client.ListDatabaseNames(cancellationToken);
    }

    public IAsyncCursor<string> ListDatabaseNames(
        ListDatabaseNamesOptions options,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _client.ListDatabaseNames(session, options, cancellationToken);
        }

        return _client.ListDatabaseNames(options, cancellationToken);
    }

    public IAsyncCursor<string> ListDatabaseNames(
        IClientSessionHandle session,
        CancellationToken cancellationToken = default)
    {
        return _client.ListDatabaseNames(session, cancellationToken);
    }

    public IAsyncCursor<string> ListDatabaseNames(
        IClientSessionHandle session,
        ListDatabaseNamesOptions options,
        CancellationToken cancellationToken = default)
    {
        return _client.ListDatabaseNames(session, options, cancellationToken);
    }

    public Task<IAsyncCursor<string>> ListDatabaseNamesAsync(
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _client.ListDatabaseNamesAsync(session, cancellationToken);
        }

        return _client.ListDatabaseNamesAsync(cancellationToken);
    }

    public Task<IAsyncCursor<string>> ListDatabaseNamesAsync(
        ListDatabaseNamesOptions options,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _client.ListDatabaseNamesAsync(session, cancellationToken);
        }

        return _client.ListDatabaseNamesAsync(options, cancellationToken);
    }

    public Task<IAsyncCursor<string>> ListDatabaseNamesAsync(
        IClientSessionHandle session,
        CancellationToken cancellationToken = default)
    {
        return _client.ListDatabaseNamesAsync(session, cancellationToken);
    }

    public Task<IAsyncCursor<string>> ListDatabaseNamesAsync(
        IClientSessionHandle session,
        ListDatabaseNamesOptions options,
        CancellationToken cancellationToken = default)
    {
        return _client.ListDatabaseNamesAsync(session, options, cancellationToken);
    }

    public IAsyncCursor<BsonDocument> ListDatabases(
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _client.ListDatabases(session, cancellationToken);
        }

        return _client.ListDatabases(cancellationToken);
    }

    public IAsyncCursor<BsonDocument> ListDatabases(
        ListDatabasesOptions options,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _client.ListDatabases(session, options, cancellationToken);
        }

        return _client.ListDatabases(options, cancellationToken);
    }

    public IAsyncCursor<BsonDocument> ListDatabases(
        IClientSessionHandle session,
        CancellationToken cancellationToken = default)
    {
        return _client.ListDatabases(session, cancellationToken);
    }

    public IAsyncCursor<BsonDocument> ListDatabases(
        IClientSessionHandle session,
        ListDatabasesOptions options,
        CancellationToken cancellationToken = default)
    {
        return _client.ListDatabases(session, options, cancellationToken);
    }

    public Task<IAsyncCursor<BsonDocument>> ListDatabasesAsync(
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _client.ListDatabasesAsync(session, cancellationToken);
        }

        return _client.ListDatabasesAsync(cancellationToken);
    }

    public Task<IAsyncCursor<BsonDocument>> ListDatabasesAsync(
        ListDatabasesOptions options,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _client.ListDatabasesAsync(session, options, cancellationToken);
        }

        return _client.ListDatabasesAsync(options, cancellationToken);
    }

    public Task<IAsyncCursor<BsonDocument>> ListDatabasesAsync(
        IClientSessionHandle session,
        CancellationToken cancellationToken = default)
    {
        return _client.ListDatabasesAsync(session, cancellationToken);
    }

    public Task<IAsyncCursor<BsonDocument>> ListDatabasesAsync(
        IClientSessionHandle session,
        ListDatabasesOptions options,
        CancellationToken cancellationToken = default)
    {
        return _client.ListDatabasesAsync(session, options, cancellationToken);
    }

    public IClientSessionHandle StartSession(
        ClientSessionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _client.StartSession(options, cancellationToken);
    }

    public Task<IClientSessionHandle> StartSessionAsync(
        ClientSessionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _client.StartSessionAsync(options, cancellationToken);
    }

    public IChangeStreamCursor<TResult> Watch<TResult>(
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _client.Watch(session, pipeline, options, cancellationToken);
        }

        return _client.Watch(pipeline, options, cancellationToken);
    }

    public IChangeStreamCursor<TResult> Watch<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _client.Watch(session, pipeline, options, cancellationToken);
    }

    public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _client.WatchAsync(session, pipeline, options, cancellationToken);
        }

        return _client.WatchAsync(pipeline, options, cancellationToken);
    }

    public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _client.WatchAsync(session, pipeline, options, cancellationToken);
    }

    public IMongoClient WithReadConcern(ReadConcern readConcern)
    {
        return _client.WithReadConcern(readConcern).AsTransactionClient();
    }

    public IMongoClient WithReadPreference(ReadPreference readPreference)
    {
        return _client.WithReadPreference(readPreference).AsTransactionClient();
    }

    public IMongoClient WithWriteConcern(WriteConcern writeConcern)
    {
        return _client.WithWriteConcern(writeConcern).AsTransactionClient();
    }

    public ICluster Cluster => _client.Cluster;

    public MongoClientSettings Settings => _client.Settings;

    private bool TryGetSession(out IClientSessionHandle sessionHandle)
    {
        if (_clientSessionHandle is { } clientSessionHandle)
        {
            sessionHandle = clientSessionHandle;
            return true;
        }

        return TransactionStore.TryGetSession(
            _client, out sessionHandle);
    }            
}
