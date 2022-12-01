using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.Prime.Extensions
{
    public static class MongoCollectionExtensions
    {
        /// <summary>
        /// Deletes all entries within a collection.
        /// The collection is not dropped and all indexes stay.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection to clean.</param>
        public static void CleanCollection<TDocument>(
            this IMongoCollection<TDocument> collection)
        {
            collection.DeleteMany(
                FilterDefinition<TDocument>.Empty);
        }

        /// <summary>
        /// Deletes all entries within a collection.
        /// The collection is not dropped and all indexes stay.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection to clean.</param>
        public static async Task CleanCollectionAsync<TDocument>(
            this IMongoCollection<TDocument> collection,
            CancellationToken cancellationToken = default)
        {
            await collection.DeleteManyAsync(
                FilterDefinition<TDocument>.Empty,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Counts all the documents of a collection
        /// </summary>
        /// <returns>The document count.</returns>
        public static long CountDocuments<TDocument>(
            this IMongoCollection<TDocument> collection,
            CountOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return collection.CountDocuments(
                FilterDefinition<TDocument>.Empty, options, cancellationToken);
        }

        /// <summary>
        /// Counts all the documents of a collection
        /// </summary>
        /// <returns>The document count.</returns>
        public static Task<long> CountDocumentsAsync<TDocument>(
            this IMongoCollection<TDocument> collection,
            CountOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return collection.CountDocumentsAsync(
                FilterDefinition<TDocument>.Empty, options, cancellationToken);
        }

        /// <summary>
        /// Reads all entries of a collection and returns a list of it.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection to dump.</param>
        public static IEnumerable<TDocument> Dump<TDocument>(
            this IMongoCollection<TDocument> collection)
        {
            SortDefinition<TDocument> sortDefinition =
                Builders<TDocument>.Sort.Ascending("_id");

            return collection
                .Find(FilterDefinition<TDocument>.Empty)
                .Sort(sortDefinition)
                .ToList();
        }

        public static Task InsertOneAsync<TDocument>(
            this IMongoCollection<TDocument> collection,
            TDocument document,
            IClientSessionHandle? session = null,
            InsertOneOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (session is { })
            {
                return collection.InsertOneAsync(
                    session, document, options, cancellationToken);
            }

            return collection.InsertOneAsync(
                document, options, cancellationToken);
        }

        public static Task InsertManyAsync<TDocument>(
            this IMongoCollection<TDocument> collection,
            IEnumerable<TDocument> documents,
            IClientSessionHandle? session = null,
            InsertManyOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (session is { })
            {
                return collection.InsertManyAsync(
                    session, documents, options, cancellationToken);
            }

            return collection.InsertManyAsync(
                documents, options, cancellationToken);
        }

        public static string? Explain<T>(
            this IMongoCollection<T> collection,
            FilterDefinition<T> filter)
        {
            var options = new FindOptions
            {
                Modifiers = new BsonDocument("$explain", true)
            };

            return Explain(collection, filter, options);
        }

        public static string? Explain<T>(
            this IMongoCollection<T> collection,
            FilterDefinition<T> filter,
            FindOptions findOptions)
        {
            findOptions.Modifiers =
                new BsonDocument("$explain", true);

            string? explain = collection
                .Find(filter, findOptions)
                .Project(new BsonDocument())
                .FirstOrDefault()
                ?.ToJson();

            return explain;
        }
    }
}
