using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoDB.Prime.Extensions
{
    public static class AsyncCursorSourceExtensions
    {
        public static Task<Dictionary<TKey, TDocument>> ToDictionaryAsync<TDocument, TKey>(
            this IAsyncCursorSource<TDocument> source,
            Func<TDocument, TKey> keySelector,
            CancellationToken cancellationToken = default) where TKey : notnull
        {
            return ToDictionaryAsync(source, keySelector, 0, cancellationToken);
        }

        public static async Task<Dictionary<TKey, TDocument>> ToDictionaryAsync<TDocument, TKey>(
            this IAsyncCursorSource<TDocument> source,
            Func<TDocument, TKey> keySelector,
            int capacity,
            CancellationToken cancellationToken = default) where TKey : notnull
        {
            var documents = new Dictionary<TKey, TDocument>(capacity);

            using IAsyncCursor<TDocument> cursor =
                await source.ToCursorAsync(cancellationToken).ConfigureAwait(false);

            while (await cursor.MoveNextAsync(cancellationToken).ConfigureAwait(false))
            {
                foreach (TDocument document in cursor.Current)
                {
                    documents.Add(keySelector(document), document);
                }
            }

            return documents;
        }
    }
}
