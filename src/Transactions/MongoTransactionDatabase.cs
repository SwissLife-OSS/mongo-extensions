using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.Extensions.Transactions;

public class MongoTransactionDatabase : IMongoDatabase
{
    private readonly IMongoDatabase _database;
    private readonly IClientSessionHandle? _clientSessionHandle;

    public MongoTransactionDatabase(IMongoDatabase database)
    {
        _database = database;
    }

    public MongoTransactionDatabase(
        IMongoDatabase database,
        IClientSessionHandle clientSessionHandle)
    {
        _database = database;
        _clientSessionHandle = clientSessionHandle;
    }

    public IAsyncCursor<TResult> Aggregate<TResult>(
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return Aggregate(session, pipeline, options, cancellationToken);
        }

        return _database.Aggregate(pipeline, options, cancellationToken);
    }

    public IAsyncCursor<TResult> Aggregate<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _database.Aggregate(session, pipeline, options, cancellationToken);
    }

    public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _database.AggregateAsync(session, pipeline, options, cancellationToken);
        }

        return _database.AggregateAsync(pipeline, options, cancellationToken);
    }

    public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _database.AggregateAsync(session, pipeline, options, cancellationToken);
    }

    public void AggregateToCollection<TResult>(
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            _database.AggregateToCollection(session, pipeline, options, cancellationToken);
            return;
        }

        _database.AggregateToCollection(pipeline, options, cancellationToken);
    }

    public void AggregateToCollection<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _database.AggregateToCollection(session, pipeline, options, cancellationToken);
    }

    public Task AggregateToCollectionAsync<TResult>(
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _database
                .AggregateToCollectionAsync(session, pipeline, options, cancellationToken);
        }

        return _database.AggregateToCollectionAsync(pipeline, options, cancellationToken);
    }

    public Task AggregateToCollectionAsync<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _database
            .AggregateToCollectionAsync(session, pipeline, options, cancellationToken);
    }

    public void CreateCollection(
        string name,
        CreateCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            _database.CreateCollection(session, name, options, cancellationToken);
            return;
        }

        _database.CreateCollection(name, options, cancellationToken);
    }

    public void CreateCollection(
        IClientSessionHandle session,
        string name,
        CreateCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _database.CreateCollection(session, name, options, cancellationToken);
    }

    public Task CreateCollectionAsync(
        string name,
        CreateCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _database.CreateCollectionAsync(session, name, options, cancellationToken);
        }

        return _database.CreateCollectionAsync(name, options, cancellationToken);
    }

    public Task CreateCollectionAsync(
        IClientSessionHandle session,
        string name,
        CreateCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _database.CreateCollectionAsync(session, name, options, cancellationToken);
    }

    public void CreateView<TDocument, TResult>(
        string viewName,
        string viewOn,
        PipelineDefinition<TDocument, TResult> pipeline,
        CreateViewOptions<TDocument>? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            _database
                .CreateView(session, viewName, viewOn, pipeline, options, cancellationToken);
            return;
        }

        _database.CreateView(viewName, viewOn, pipeline, options, cancellationToken);
    }

    public void CreateView<TDocument, TResult>(
        IClientSessionHandle session,
        string viewName,
        string viewOn,
        PipelineDefinition<TDocument, TResult> pipeline,
        CreateViewOptions<TDocument>? options = null,
        CancellationToken cancellationToken = default)
    {
        _database.CreateView(session, viewName, viewOn, pipeline, options, cancellationToken);
    }

    public Task CreateViewAsync<TDocument, TResult>(
        string viewName,
        string viewOn,
        PipelineDefinition<TDocument, TResult> pipeline,
        CreateViewOptions<TDocument>? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _database.CreateViewAsync(
                session,
                viewName,
                viewOn,
                pipeline,
                options,
                cancellationToken);
        }

        return _database
            .CreateViewAsync(viewName, viewOn, pipeline, options, cancellationToken);
    }

    public Task CreateViewAsync<TDocument, TResult>(
        IClientSessionHandle session,
        string viewName,
        string viewOn,
        PipelineDefinition<TDocument, TResult> pipeline,
        CreateViewOptions<TDocument>? options = null,
        CancellationToken cancellationToken = default)
    {
        return _database
            .CreateViewAsync(session, viewName, viewOn, pipeline, options, cancellationToken);
    }

    public void DropCollection(
        string name,
        DropCollectionOptions options,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            _database.DropCollection(session, name, options, cancellationToken);
            return;
        }

        _database.DropCollection(name, options, cancellationToken);
    }

    public void DropCollection(
        IClientSessionHandle session,
        string name,
        DropCollectionOptions options,
        CancellationToken cancellationToken = default)
    {
        _database.DropCollectionAsync(session, name, options, cancellationToken);
    }

    public Task DropCollectionAsync(
        string name,
        DropCollectionOptions options,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _database.DropCollectionAsync(session, name, options, cancellationToken);
        }

        return _database.DropCollectionAsync(name, options, cancellationToken);
    }

    public Task DropCollectionAsync(
        IClientSessionHandle session,
        string name,
        DropCollectionOptions options,
        CancellationToken cancellationToken = default)
    {
        return _database.DropCollectionAsync(session, name, options, cancellationToken);
    }

    public void DropCollection(string name, CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            _database.DropCollection(session, name, cancellationToken);
            return;
        }

        _database.DropCollection(name, cancellationToken);
    }

    public void DropCollection(
        IClientSessionHandle session,
        string name,
        CancellationToken cancellationToken = default)
    {
        _database.DropCollection(session, name, cancellationToken);
    }

    public Task DropCollectionAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _database.DropCollectionAsync(session, name, cancellationToken);
        }

        return _database.DropCollectionAsync(name, cancellationToken);
    }

    public Task DropCollectionAsync(
        IClientSessionHandle session,
        string name,
        CancellationToken cancellationToken = default)
    {
        return _database.DropCollectionAsync(session, name, cancellationToken);
    }

    public IMongoCollection<TDocument> GetCollection<TDocument>(
        string name,
        MongoCollectionSettings? settings = null)
    {
        return _database.GetCollection<TDocument>(name, settings).AsTransactionCollection();
    }

    public IAsyncCursor<string> ListCollectionNames(
        ListCollectionNamesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _database.ListCollectionNames(session, options, cancellationToken);
        }

        return _database.ListCollectionNames(options, cancellationToken);
    }

    public IAsyncCursor<string> ListCollectionNames(
        IClientSessionHandle session,
        ListCollectionNamesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _database.ListCollectionNames(session, options, cancellationToken);
    }

    public Task<IAsyncCursor<string>> ListCollectionNamesAsync(
        ListCollectionNamesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _database.ListCollectionNamesAsync(session, options, cancellationToken);
        }

        return _database.ListCollectionNamesAsync(options, cancellationToken);
    }

    public Task<IAsyncCursor<string>> ListCollectionNamesAsync(
        IClientSessionHandle session,
        ListCollectionNamesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _database.ListCollectionNamesAsync(session, options, cancellationToken);
    }

    public IAsyncCursor<BsonDocument> ListCollections(
        ListCollectionsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _database.ListCollections(session, options, cancellationToken);
        }

        return _database.ListCollections(options, cancellationToken);
    }

    public IAsyncCursor<BsonDocument> ListCollections(
        IClientSessionHandle session,
        ListCollectionsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _database.ListCollections(session, options, cancellationToken);
    }

    public Task<IAsyncCursor<BsonDocument>> ListCollectionsAsync(
        ListCollectionsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _database.ListCollectionsAsync(session, options, cancellationToken);
        }

        return _database.ListCollectionsAsync(options, cancellationToken);
    }

    public Task<IAsyncCursor<BsonDocument>> ListCollectionsAsync(
        IClientSessionHandle session,
        ListCollectionsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _database.ListCollectionsAsync(session, options, cancellationToken);
    }

    public void RenameCollection(
        string oldName,
        string newName,
        RenameCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            _database.RenameCollection(session, oldName, newName, options, cancellationToken);
            return;
        }

        _database.RenameCollection(oldName, newName, options, cancellationToken);
    }

    public void RenameCollection(
        IClientSessionHandle session,
        string oldName,
        string newName,
        RenameCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _database.RenameCollection(session, oldName, newName, options, cancellationToken);
    }

    public Task RenameCollectionAsync(
        string oldName,
        string newName,
        RenameCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _database
                .RenameCollectionAsync(session, oldName, newName, options, cancellationToken);
        }

        return _database.RenameCollectionAsync(oldName, newName, options, cancellationToken);
    }

    public Task RenameCollectionAsync(
        IClientSessionHandle session,
        string oldName,
        string newName,
        RenameCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _database
            .RenameCollectionAsync(session, oldName, newName, options, cancellationToken);
    }

    public TResult RunCommand<TResult>(
        Command<TResult> command,
        ReadPreference? readPreference = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _database.RunCommand(session, command, readPreference, cancellationToken);
        }

        return _database.RunCommand(command, readPreference, cancellationToken);
    }

    public TResult RunCommand<TResult>(
        IClientSessionHandle session,
        Command<TResult> command,
        ReadPreference? readPreference = null,
        CancellationToken cancellationToken = default)
    {
        return _database.RunCommand(session, command, readPreference, cancellationToken);
    }

    public Task<TResult> RunCommandAsync<TResult>(
        Command<TResult> command,
        ReadPreference? readPreference = null,
        CancellationToken cancellationToken = default)
    {
        return _database.RunCommandAsync(command, readPreference, cancellationToken);
    }

    public Task<TResult> RunCommandAsync<TResult>(
        IClientSessionHandle session,
        Command<TResult> command,
        ReadPreference? readPreference = null,
        CancellationToken cancellationToken = default)
    {
        return _database.RunCommandAsync(session, command, readPreference, cancellationToken);
    }

    public IChangeStreamCursor<TResult> Watch<TResult>(
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _database.Watch(session, pipeline, options, cancellationToken);
        }

        return _database.Watch(pipeline, options, cancellationToken);
    }

    public IChangeStreamCursor<TResult> Watch<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _database.Watch(session, pipeline, options, cancellationToken);
    }

    public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out IClientSessionHandle? session))
        {
            return _database.WatchAsync(session, pipeline, options, cancellationToken);
        }

        return _database.WatchAsync(pipeline, options, cancellationToken);
    }

    public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return _database.WatchAsync(session, pipeline, options, cancellationToken);
    }

    public IMongoDatabase WithReadConcern(ReadConcern readConcern)
    {
        return _database.WithReadConcern(readConcern).AsTransactionDatabase();
    }

    public IMongoDatabase WithReadPreference(ReadPreference readPreference)
    {
        return _database.WithReadPreference(readPreference).AsTransactionDatabase();
    }

    public IMongoDatabase WithWriteConcern(WriteConcern writeConcern)
    {
        return _database.WithWriteConcern(writeConcern).AsTransactionDatabase();
    }

    public IMongoClient Client => _database.Client;

    public DatabaseNamespace DatabaseNamespace => _database.DatabaseNamespace;

    public MongoDatabaseSettings Settings => _database.Settings;

    private bool TryGetSession(out IClientSessionHandle sessionHandle)
    {
        if (_clientSessionHandle is { } clientSessionHandle)
        {
            sessionHandle = clientSessionHandle;
            return true;
        }

        return TransactionStore.TryGetSession(
            _database.Client, out sessionHandle);
    }        
}
