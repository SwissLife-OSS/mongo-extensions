using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

#pragma warning disable 618

namespace MongoDB.Extensions.Transactions
{
    public class MongoTransactionCollection<T> : IMongoCollection<T>
    {
        private readonly ConcurrentDictionary<string, IClientSessionHandle> _sessions = new();
        private readonly IMongoCollection<T> _collection;
        private readonly IMongoClient _client;

        public MongoTransactionCollection(IMongoCollection<T> collection)
        {
            _collection = collection;
            _client = collection.Database.Client;
        }

        private IClientSessionHandle GetOrCreateTransaction(string id) =>
            _sessions.GetOrAdd(id, CreateAndRegister);

        private IClientSessionHandle CreateAndRegister(string id)
        {
            if (Transaction.Current is null)
            {
                throw new TransactionException("Cannot open a transaction without a valid scope");
            }

            IClientSessionHandle? session = _client.StartSession();
            session.StartTransaction();
            MongoDbEnlistmentScope enlistment = new(session, Unregister);

            Transaction.Current.EnlistVolatile(enlistment, EnlistmentOptions.None);

            return session;

            void Unregister()
            {
                if (_sessions.TryRemove(id, out session))
                {
                    session.Dispose();
                }
            }
        }

        private bool TryGetSession(out IClientSessionHandle sessionHandle)
        {
            if (Transaction.Current?.TransactionInformation.LocalIdentifier is { } id)
            {
                sessionHandle = GetOrCreateTransaction(id);
                return true;
            }

            sessionHandle = null!;
            return false;
        }

        public IAsyncCursor<TResult> Aggregate<TResult>(
            PipelineDefinition<T, TResult> pipeline,
            AggregateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return Aggregate(session, pipeline, options, cancellationToken);
            }

            return _collection.Aggregate(pipeline, options, cancellationToken);
        }

        public IAsyncCursor<TResult> Aggregate<TResult>(
            IClientSessionHandle session,
            PipelineDefinition<T, TResult> pipeline,
            AggregateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.Aggregate(session, pipeline, options, cancellationToken);
        }

        public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(
            PipelineDefinition<T, TResult> pipeline,
            AggregateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return AggregateAsync(session, pipeline, options, cancellationToken);
            }

            return _collection.AggregateAsync(pipeline, options, cancellationToken);
        }

        public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(
            IClientSessionHandle session,
            PipelineDefinition<T, TResult> pipeline,
            AggregateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.AggregateAsync(session, pipeline, options, cancellationToken);
        }

        public void AggregateToCollection<TResult>(
            PipelineDefinition<T, TResult> pipeline,
            AggregateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                AggregateToCollection(session, pipeline, options, cancellationToken);
            }

            _collection.AggregateToCollection(pipeline, options, cancellationToken);
        }

        public void AggregateToCollection<TResult>(
            IClientSessionHandle session,
            PipelineDefinition<T, TResult> pipeline,
            AggregateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _collection.AggregateToCollection(session, pipeline, options, cancellationToken);
        }

        public Task AggregateToCollectionAsync<TResult>(
            PipelineDefinition<T, TResult> pipeline,
            AggregateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return AggregateToCollectionAsync(session, pipeline, options, cancellationToken);
            }

            return _collection.AggregateToCollectionAsync(pipeline, options, cancellationToken);
        }

        public Task AggregateToCollectionAsync<TResult>(
            IClientSessionHandle session,
            PipelineDefinition<T, TResult> pipeline,
            AggregateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.AggregateToCollectionAsync(session,
                pipeline,
                options,
                cancellationToken);
        }

        public BulkWriteResult<T> BulkWrite(
            IEnumerable<WriteModel<T>> requests,
            BulkWriteOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return BulkWrite(session, requests, options, cancellationToken);
            }

            return _collection.BulkWrite(requests, options, cancellationToken);
        }

        public BulkWriteResult<T> BulkWrite(
            IClientSessionHandle session,
            IEnumerable<WriteModel<T>> requests,
            BulkWriteOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.BulkWrite(session, requests, options, cancellationToken);
        }

        public Task<BulkWriteResult<T>> BulkWriteAsync(
            IEnumerable<WriteModel<T>> requests,
            BulkWriteOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return BulkWriteAsync(session, requests, options, cancellationToken);
            }

            return _collection.BulkWriteAsync(requests, options, cancellationToken);
        }

        public Task<BulkWriteResult<T>> BulkWriteAsync(
            IClientSessionHandle session,
            IEnumerable<WriteModel<T>> requests,
            BulkWriteOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.BulkWriteAsync(session, requests, options, cancellationToken);
        }

        public long Count(
            FilterDefinition<T> filter,
            CountOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return Count(session, filter, options, cancellationToken);
            }

            return _collection.Count(filter, options, cancellationToken);
        }

        public long Count(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            CountOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.Count(session, filter, options, cancellationToken);
        }

        public Task<long> CountAsync(
            FilterDefinition<T> filter,
            CountOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return CountAsync(session, filter, options, cancellationToken);
            }

            return _collection.CountAsync(filter, options, cancellationToken);
        }

        public Task<long> CountAsync(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            CountOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.CountAsync(session, filter, options, cancellationToken);
        }

        public long CountDocuments(
            FilterDefinition<T> filter,
            CountOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return CountDocuments(session, filter, options, cancellationToken);
            }

            return _collection.CountDocuments(filter, options, cancellationToken);
        }

        public long CountDocuments(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            CountOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.CountDocuments(session, filter, options, cancellationToken);
        }

        public Task<long> CountDocumentsAsync(
            FilterDefinition<T> filter,
            CountOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return CountDocumentsAsync(session, filter, options, cancellationToken);
            }

            return _collection.CountDocumentsAsync(filter, options, cancellationToken);
        }

        public Task<long> CountDocumentsAsync(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            CountOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.CountDocumentsAsync(session, filter, options, cancellationToken);
        }

        public DeleteResult DeleteMany(
            FilterDefinition<T> filter,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return DeleteMany(session, filter, cancellationToken: cancellationToken);
            }

            return _collection.DeleteMany(filter, cancellationToken);
        }

        public DeleteResult DeleteMany(
            FilterDefinition<T> filter,
            DeleteOptions options,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return DeleteMany(session, filter, options, cancellationToken);
            }

            return _collection.DeleteMany(filter, options, cancellationToken);
        }

        public DeleteResult DeleteMany(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            DeleteOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.DeleteMany(session, filter, options, cancellationToken);
        }

        public Task<DeleteResult> DeleteManyAsync(
            FilterDefinition<T> filter,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return DeleteManyAsync(session, filter, cancellationToken: cancellationToken);
            }

            return _collection.DeleteManyAsync(filter, cancellationToken);
        }

        public Task<DeleteResult> DeleteManyAsync(
            FilterDefinition<T> filter,
            DeleteOptions options,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return DeleteManyAsync(session, filter, options, cancellationToken);
            }

            return _collection.DeleteManyAsync(filter, options, cancellationToken);
        }

        public Task<DeleteResult> DeleteManyAsync(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            DeleteOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.DeleteManyAsync(session, filter, options, cancellationToken);
        }

        public DeleteResult DeleteOne(
            FilterDefinition<T> filter,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return DeleteOne(session, filter, cancellationToken: cancellationToken);
            }

            return _collection.DeleteOne(filter, cancellationToken);
        }

        public DeleteResult DeleteOne(
            FilterDefinition<T> filter,
            DeleteOptions options,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return DeleteOne(session, filter, options, cancellationToken);
            }

            return _collection.DeleteOne(filter, options, cancellationToken);
        }

        public DeleteResult DeleteOne(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            DeleteOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.DeleteOne(session, filter, options, cancellationToken);
        }

        public Task<DeleteResult> DeleteOneAsync(
            FilterDefinition<T> filter,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return DeleteOneAsync(session, filter, cancellationToken: cancellationToken);
            }

            return _collection.DeleteOneAsync(filter, cancellationToken);
        }

        public Task<DeleteResult> DeleteOneAsync(
            FilterDefinition<T> filter,
            DeleteOptions options,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return DeleteOneAsync(session, filter, options, cancellationToken);
            }

            return _collection.DeleteOneAsync(filter, options, cancellationToken);
        }

        public Task<DeleteResult> DeleteOneAsync(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            DeleteOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.DeleteOneAsync(session, filter, options, cancellationToken);
        }

        public IAsyncCursor<TField> Distinct<TField>(
            FieldDefinition<T, TField> field,
            FilterDefinition<T> filter,
            DistinctOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return Distinct(session, field, filter, options, cancellationToken);
            }

            return _collection.Distinct(field, filter, options, cancellationToken);
        }

        public IAsyncCursor<TField> Distinct<TField>(
            IClientSessionHandle session,
            FieldDefinition<T, TField> field,
            FilterDefinition<T> filter,
            DistinctOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.Distinct(session, field, filter, options, cancellationToken);
        }

        public Task<IAsyncCursor<TField>> DistinctAsync<TField>(
            FieldDefinition<T, TField> field,
            FilterDefinition<T> filter,
            DistinctOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return DistinctAsync(session, field, filter, options, cancellationToken);
            }

            return _collection.DistinctAsync(field, filter, options, cancellationToken);
        }

        public Task<IAsyncCursor<TField>> DistinctAsync<TField>(
            IClientSessionHandle session,
            FieldDefinition<T, TField> field,
            FilterDefinition<T> filter,
            DistinctOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.DistinctAsync(session, field, filter, options, cancellationToken);
        }

        public long EstimatedDocumentCount(
            EstimatedDocumentCountOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.EstimatedDocumentCount(options, cancellationToken);
        }

        public Task<long> EstimatedDocumentCountAsync(
            EstimatedDocumentCountOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.EstimatedDocumentCountAsync(options, cancellationToken);
        }

        public IAsyncCursor<TProjection> FindSync<TProjection>(
            FilterDefinition<T> filter,
            FindOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return FindSync(session, filter, options, cancellationToken);
            }

            return _collection.FindSync(filter, options, cancellationToken);
        }

        public IAsyncCursor<TProjection> FindSync<TProjection>(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            FindOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.FindSync(session, filter, options, cancellationToken);
        }

        public Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(
            FilterDefinition<T> filter,
            FindOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return FindAsync(session, filter, options, cancellationToken);
            }

            return _collection.FindAsync(filter, options, cancellationToken);
        }

        public Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            FindOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.FindAsync(session, filter, options, cancellationToken);
        }

        public TProjection FindOneAndDelete<TProjection>(
            FilterDefinition<T> filter,
            FindOneAndDeleteOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return FindOneAndDelete(session, filter, options, cancellationToken);
            }

            return _collection.FindOneAndDelete(filter, options, cancellationToken);
        }

        public TProjection FindOneAndDelete<TProjection>(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            FindOneAndDeleteOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.FindOneAndDelete(session, filter, options, cancellationToken);
        }

        public Task<TProjection> FindOneAndDeleteAsync<TProjection>(
            FilterDefinition<T> filter,
            FindOneAndDeleteOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return FindOneAndDeleteAsync(session, filter, options, cancellationToken);
            }

            return _collection.FindOneAndDeleteAsync(filter, options, cancellationToken);
        }

        public Task<TProjection> FindOneAndDeleteAsync<TProjection>(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            FindOneAndDeleteOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.FindOneAndDeleteAsync(session, filter, options, cancellationToken);
        }

        public TProjection FindOneAndReplace<TProjection>(
            FilterDefinition<T> filter,
            T replacement,
            FindOneAndReplaceOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return FindOneAndReplace(session, filter, replacement, options, cancellationToken);
            }

            return _collection.FindOneAndReplace(filter, replacement, options, cancellationToken);
        }

        public TProjection FindOneAndReplace<TProjection>(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            T replacement,
            FindOneAndReplaceOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.FindOneAndReplace(session,
                filter,
                replacement,
                options,
                cancellationToken);
        }

        public Task<TProjection> FindOneAndReplaceAsync<TProjection>(
            FilterDefinition<T> filter,
            T replacement,
            FindOneAndReplaceOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return FindOneAndReplaceAsync(session,
                    filter,
                    replacement,
                    options,
                    cancellationToken);
            }

            return _collection.FindOneAndReplaceAsync(filter,
                replacement,
                options,
                cancellationToken);
        }

        public Task<TProjection> FindOneAndReplaceAsync<TProjection>(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            T replacement,
            FindOneAndReplaceOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.FindOneAndReplaceAsync(session,
                filter,
                replacement,
                options,
                cancellationToken);
        }

        public TProjection FindOneAndUpdate<TProjection>(
            FilterDefinition<T> filter,
            UpdateDefinition<T> update,
            FindOneAndUpdateOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return FindOneAndUpdate(session, filter, update, options, cancellationToken);
            }

            return _collection.FindOneAndUpdate(filter, update, options, cancellationToken);
        }

        public TProjection FindOneAndUpdate<TProjection>(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            UpdateDefinition<T> update,
            FindOneAndUpdateOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.FindOneAndUpdate(session,
                filter,
                update,
                options,
                cancellationToken);
        }

        public Task<TProjection> FindOneAndUpdateAsync<TProjection>(
            FilterDefinition<T> filter,
            UpdateDefinition<T> update,
            FindOneAndUpdateOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return FindOneAndUpdateAsync(session, filter, update, options, cancellationToken);
            }

            return _collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        }

        public Task<TProjection> FindOneAndUpdateAsync<TProjection>(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            UpdateDefinition<T> update,
            FindOneAndUpdateOptions<T, TProjection>? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.FindOneAndUpdateAsync(session,
                filter,
                update,
                options,
                cancellationToken);
        }

        public void InsertOne(
            T document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                InsertOne(session, document, options, cancellationToken);
            }

            _collection.InsertOne(document, options, cancellationToken);
        }

        public void InsertOne(
            IClientSessionHandle session,
            T document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _collection.InsertOne(session, document, options, cancellationToken);
        }

        public Task InsertOneAsync(T document, CancellationToken cancellationToken)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return InsertOneAsync(session, document, cancellationToken: cancellationToken);
            }

            return _collection.InsertOneAsync(document, cancellationToken);
        }

        public Task InsertOneAsync(
            T document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return InsertOneAsync(session, document, options, cancellationToken);
            }

            return _collection.InsertOneAsync(document, options, cancellationToken);
        }

        public Task InsertOneAsync(
            IClientSessionHandle session,
            T document,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.InsertOneAsync(session, document, options, cancellationToken);
        }

        public void InsertMany(
            IEnumerable<T> documents,
            InsertManyOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                // ReSharper disable once PossibleMultipleEnumeration
                InsertMany(session, documents, options, cancellationToken);
            }

            // ReSharper disable once PossibleMultipleEnumeration
            _collection.InsertMany(documents, options, cancellationToken);
        }

        public void InsertMany(
            IClientSessionHandle session,
            IEnumerable<T> documents,
            InsertManyOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _collection.InsertMany(session, documents, options, cancellationToken);
        }

        public Task InsertManyAsync(
            IEnumerable<T> documents,
            InsertManyOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return InsertManyAsync(session, documents, options, cancellationToken);
            }

            return _collection.InsertManyAsync(documents, options, cancellationToken);
        }

        public Task InsertManyAsync(
            IClientSessionHandle session,
            IEnumerable<T> documents,
            InsertManyOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.InsertManyAsync(session, documents, options, cancellationToken);
        }

        public IAsyncCursor<TResult> MapReduce<TResult>(
            BsonJavaScript map,
            BsonJavaScript reduce,
            MapReduceOptions<T, TResult>? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return MapReduce(session, map, reduce, options, cancellationToken);
            }

            return _collection.MapReduce(map, reduce, options, cancellationToken);
        }

        public IAsyncCursor<TResult> MapReduce<TResult>(
            IClientSessionHandle session,
            BsonJavaScript map,
            BsonJavaScript reduce,
            MapReduceOptions<T, TResult>? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.MapReduce(session, map, reduce, options, cancellationToken);
        }

        public Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(
            BsonJavaScript map,
            BsonJavaScript reduce,
            MapReduceOptions<T, TResult>? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return MapReduceAsync(session, map, reduce, options, cancellationToken);
            }

            return _collection.MapReduceAsync(map, reduce, options, cancellationToken);
        }

        public Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(
            IClientSessionHandle session,
            BsonJavaScript map,
            BsonJavaScript reduce,
            MapReduceOptions<T, TResult>? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.MapReduceAsync(session, map, reduce, options, cancellationToken);
        }

        public IFilteredMongoCollection<TDerivedDocument> OfType<TDerivedDocument>()
            where TDerivedDocument : T
        {
            return _collection.OfType<TDerivedDocument>();
        }

        public ReplaceOneResult ReplaceOne(
            FilterDefinition<T> filter,
            T replacement,
            ReplaceOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return ReplaceOne(session, filter, replacement, options, cancellationToken);
            }

            return _collection.ReplaceOne(filter, replacement, options, cancellationToken);
        }

        public ReplaceOneResult ReplaceOne(
            FilterDefinition<T> filter,
            T replacement,
            UpdateOptions options,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return ReplaceOne(session, filter, replacement, options, cancellationToken);
            }

            return _collection.ReplaceOne(filter, replacement, options, cancellationToken);
        }

        public ReplaceOneResult ReplaceOne(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            T replacement,
            ReplaceOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.ReplaceOne(session, filter, replacement, options, cancellationToken);
        }

        public ReplaceOneResult ReplaceOne(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            T replacement,
            UpdateOptions options,
            CancellationToken cancellationToken = default)
        {
            return _collection.ReplaceOne(session, filter, replacement, options, cancellationToken);
        }

        public Task<ReplaceOneResult> ReplaceOneAsync(
            FilterDefinition<T> filter,
            T replacement,
            ReplaceOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return ReplaceOneAsync(session, filter, replacement, options, cancellationToken);
            }

            return _collection.ReplaceOneAsync(filter, replacement, options, cancellationToken);
        }

        public Task<ReplaceOneResult> ReplaceOneAsync(
            FilterDefinition<T> filter,
            T replacement,
            UpdateOptions options,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return ReplaceOneAsync(session, filter, replacement, options, cancellationToken);
            }

            return _collection.ReplaceOneAsync(filter, replacement, options, cancellationToken);
        }

        public Task<ReplaceOneResult> ReplaceOneAsync(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            T replacement,
            ReplaceOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.ReplaceOneAsync(session,
                filter,
                replacement,
                options,
                cancellationToken);
        }

        public Task<ReplaceOneResult> ReplaceOneAsync(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            T replacement,
            UpdateOptions options,
            CancellationToken cancellationToken = default)
        {
            return _collection.ReplaceOneAsync(session,
                filter,
                replacement,
                options,
                cancellationToken);
        }

        public UpdateResult UpdateMany(
            FilterDefinition<T> filter,
            UpdateDefinition<T> update,
            UpdateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return UpdateMany(session, filter, update, options, cancellationToken);
            }

            return _collection.UpdateMany(filter, update, options, cancellationToken);
        }

        public UpdateResult UpdateMany(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            UpdateDefinition<T> update,
            UpdateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.UpdateMany(session, filter, update, options, cancellationToken);
        }

        public Task<UpdateResult> UpdateManyAsync(
            FilterDefinition<T> filter,
            UpdateDefinition<T> update,
            UpdateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return UpdateManyAsync(session, filter, update, options, cancellationToken);
            }

            return _collection.UpdateManyAsync(filter, update, options, cancellationToken);
        }

        public Task<UpdateResult> UpdateManyAsync(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            UpdateDefinition<T> update,
            UpdateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.UpdateManyAsync(session, filter, update, options, cancellationToken);
        }

        public UpdateResult UpdateOne(
            FilterDefinition<T> filter,
            UpdateDefinition<T> update,
            UpdateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return UpdateOne(session, filter, update, options, cancellationToken);
            }

            return _collection.UpdateOne(filter, update, options, cancellationToken);
        }

        public UpdateResult UpdateOne(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            UpdateDefinition<T> update,
            UpdateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.UpdateOne(session, filter, update, options, cancellationToken);
        }

        public Task<UpdateResult> UpdateOneAsync(
            FilterDefinition<T> filter,
            UpdateDefinition<T> update,
            UpdateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return UpdateOneAsync(session, filter, update, options, cancellationToken);
            }

            return _collection.UpdateOneAsync(filter, update, options, cancellationToken);
        }

        public Task<UpdateResult> UpdateOneAsync(
            IClientSessionHandle session,
            FilterDefinition<T> filter,
            UpdateDefinition<T> update,
            UpdateOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.UpdateOneAsync(session, filter, update, options, cancellationToken);
        }

        public IChangeStreamCursor<TResult> Watch<TResult>(
            PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline,
            ChangeStreamOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return Watch(session, pipeline, options, cancellationToken);
            }

            return _collection.Watch(pipeline, options, cancellationToken);
        }

        public IChangeStreamCursor<TResult> Watch<TResult>(
            IClientSessionHandle session,
            PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline,
            ChangeStreamOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.Watch(session, pipeline, options, cancellationToken);
        }

        public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(
            PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline,
            ChangeStreamOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (TryGetSession(out IClientSessionHandle? session))
            {
                return WatchAsync(session, pipeline, options, cancellationToken);
            }

            return _collection.WatchAsync(pipeline, options, cancellationToken);
        }

        public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(
            IClientSessionHandle session,
            PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline,
            ChangeStreamOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return _collection.WatchAsync(session, pipeline, options, cancellationToken);
        }

        public IMongoCollection<T> WithReadConcern(ReadConcern readConcern)
        {
            return _collection.WithReadConcern(readConcern).AsTransactionCollection();
        }

        public IMongoCollection<T> WithReadPreference(ReadPreference readPreference)
        {
            return _collection.WithReadPreference(readPreference).AsTransactionCollection();
        }

        public IMongoCollection<T> WithWriteConcern(WriteConcern writeConcern)
        {
            return _collection.WithWriteConcern(writeConcern).AsTransactionCollection();
        }

        public CollectionNamespace CollectionNamespace => _collection.CollectionNamespace;

        public IMongoDatabase Database => _collection.Database;

        public IBsonSerializer<T> DocumentSerializer => _collection.DocumentSerializer;

        public IMongoIndexManager<T> Indexes => _collection.Indexes;

        public MongoCollectionSettings Settings => _collection.Settings;
    }
}
