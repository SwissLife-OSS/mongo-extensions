using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.Prime.Extensions
{
    public static class MongoCollectionExtensions
    {
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
