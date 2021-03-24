using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using MongoDB.Driver;
#nullable enable

namespace MongoDB.Extensions.Context.Extensions
{
    public static class MongoCollectionExtensions
    {
        private static readonly int DefaultConcurrencyLevel = Environment.ProcessorCount;

        public static async Task<IDictionary<TId, TDocument>> GetIdBatches<TId, TDocument>(
            this IMongoCollection<TDocument> mongoCollection,
            IReadOnlyList<TId> ids,
            Func<TDocument, TId> idSelector,
            FindOptions? findOptions = null,
            CancellationToken cancellationToken = default) where TId : struct
        {
            int batchPartitionsCount = CalculateBatchPartitions(ids.Count);

            var concurrentDocuments =
                new ConcurrentDictionary<TId, TDocument>(
                    DefaultConcurrencyLevel, ids.Count);

            IList<IEnumerator<TId>> partitions = Partitioner
                .Create(ids)
                .GetPartitions(batchPartitionsCount);

            await Task.WhenAll(partitions.Select(partition =>
                Task.Run(async () =>
                {
                    using (partition)
                    {
                        IEnumerable<TId> idsBatch = partition.ToEnumerable();

                        using IAsyncCursor<TDocument> documentAsyncCursor = await mongoCollection
                            .FindIdsInAsync(idsBatch, idSelector, findOptions, cancellationToken);

                        while (await documentAsyncCursor.MoveNextAsync(cancellationToken))
                        {
                            foreach (TDocument documentBase in documentAsyncCursor.Current)
                            {
                                concurrentDocuments-
                                    .TryAdd(idSelector(documentBase), documentBase);
                            }
                        }
                    }
                }, cancellationToken)));

            return concurrentDocuments;
        }

        private static Task<IAsyncCursor<TDocument>> FindIdsInAsync<TId, TDocument>(
            this IMongoCollection<TDocument> mongoCollection,
            IEnumerable<TId> idsBatch,
            Expression<Func<TDocument, TId>> idSelector,
            FindOptions findOptions,
            CancellationToken cancellationToken) where TId : struct
        {
            FilterDefinition<TDocument> filter =
                Builders<TDocument>.Filter.In(idSelector, idsBatch);

            return mongoCollection
                .Find(filter, findOptions)
                .ToCursorAsync(cancellationToken);
        }

        public static int CalculateBatchPartitions(int documentBaseIdsCount) =>
            documentBaseIdsCount switch
            {
                > 100000 => documentBaseIdsCount / 10000,
                > 10000 => documentBaseIdsCount / 1000,
                > 1000 => documentBaseIdsCount / 500,
                > 250 => documentBaseIdsCount / 125,
                _ => 0
            };

        private static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }
}
