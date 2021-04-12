using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
#nullable enable

namespace MongoDB.Extensions.Context
{
    public static class MongoCollectionFindExtensions
    {
        private static readonly int DefaultConcurrencyLevel = Environment.ProcessorCount;

        public static async Task<IDictionary<TId, TDocument>> FindIdsAsync<TId, TDocument>(
            this IMongoCollection<TDocument> mongoCollection,
            IEnumerable<TId> idsToFind,
            Expression<Func<TDocument, TId>> idResultSelector,
            FindOptions? findOptions = null,
            int? parallelBatchSize = null,
            CancellationToken cancellationToken = default)
        {
            int allIdsCount = idsToFind.Count();

            Func<TDocument, TId> idSelectorFunc = idResultSelector.Compile();

            int batchPartitionsCount = 
                CalculateRequestPartitionCount(allIdsCount, parallelBatchSize);

            if (batchPartitionsCount <= 1)
            {
                return await mongoCollection.FindIdsInOneRequest(
                    idsToFind,
                    idSelectorFunc,
                    idResultSelector,
                    findOptions,
                    cancellationToken);
            }

            return await mongoCollection.FindIdsInParallelRequests(
                idsToFind,
                idSelectorFunc,
                idResultSelector,
                findOptions,
                allIdsCount,
                batchPartitionsCount,
                cancellationToken);
        }

        public static int CalculateRequestPartitionCount(
            int idsCount,
            int? batchSize = null)
        {
            if (idsCount == 0)
            {
                return 0;
            }

            if (batchSize is { } batchSizeValue)
            {
                if (batchSizeValue <= 0)
                {
                    return 0;
                }

                return idsCount / batchSizeValue;
            }

            return idsCount switch
            {
                >= 1000000 => 10,
                >= 500000 => idsCount / 100000,
                >= 200000 => idsCount / 50000,
                >= 100000 => idsCount / 20000,
                >= 50000 => idsCount / 10000,
                >= 20000 => idsCount / 5000,
                >= 10000 => idsCount / 2000,
                >= 5000 => idsCount / 1000,
                >= 1000 => idsCount / 500,
                >= 500 => idsCount / 250,
                >= 250 => idsCount / 125,
                _ => 1
            };
        }

        private static async Task<IDictionary<TId, TDocument>> FindIdsInParallelRequests<TId, TDocument>(
            this IMongoCollection<TDocument> mongoCollection,
            IEnumerable<TId> idsToFind,
            Func<TDocument, TId> idSelectorFunc,
            Expression<Func<TDocument, TId>> idResultSelector,
            FindOptions? findOptions,
            int allIdsCount,
            int batchPartitionsCount,
            CancellationToken cancellationToken)
        {
            var concurrentDocuments =
                new ConcurrentDictionary<TId, TDocument>(DefaultConcurrencyLevel, allIdsCount);

            IList<IEnumerator<TId>> partitions = Partitioner
                .Create(idsToFind)
                .GetPartitions(batchPartitionsCount);

            await Task.WhenAll(partitions.Select(partition =>
                Task.Run(async () => {
                    using (partition)
                    {
                        FilterDefinition<TDocument> findFilter = Builders<TDocument>
                            .Filter.In(idResultSelector, partition.ToEnumerable());

                        using IAsyncCursor<TDocument> documentAsyncCursor = await mongoCollection
                            .Find(findFilter, findOptions)
                            .ToCursorAsync(cancellationToken);

                        while (await documentAsyncCursor.MoveNextAsync(cancellationToken))
                        {
                            foreach (TDocument documentBase in documentAsyncCursor.Current)
                            {
                                if (!concurrentDocuments
                                    .TryAdd(idSelectorFunc(documentBase), documentBase))
                                {
                                    throw new ArgumentException(
                                        $"An item with the same key has already " +
                                        $"been added.Key: {idSelectorFunc(documentBase)}");
                                }
                            }
                        }
                    }}, cancellationToken)));

            return concurrentDocuments;
        }

        private static async Task<IDictionary<TId, TDocument>> FindIdsInOneRequest<TId, TDocument>(
            this IMongoCollection<TDocument> mongoCollection,
            IEnumerable<TId> idsToFind,
            Func<TDocument, TId> idSelectorFunc,
            Expression<Func<TDocument, TId>> idResultSelector,
            FindOptions? findOptions = null,
            CancellationToken cancellationToken = default)
        {
            FilterDefinition<TDocument> findFilter =
                    Builders<TDocument>.Filter.In(idResultSelector, idsToFind);

            return await mongoCollection
                .Find(findFilter, findOptions)
                .ToDictionaryAsync(idSelectorFunc, cancellationToken);
        }

        private static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }
}
