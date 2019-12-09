using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Tag = Models.Tag;

namespace SimpleBlog.DataAccess
{
    public class TagRepository : ITagRepository
    {
        private IMongoCollection<Tag> _mongoCollection;

        public TagRepository(ISimpleBlogDbContext simpleBlogDbContext)
        {
            if (simpleBlogDbContext == null)
                throw new ArgumentNullException(nameof(simpleBlogDbContext));

            _mongoCollection = simpleBlogDbContext.CreateCollection<Tag>();
        }

        public async Task<IEnumerable<Tag>> GetTagsAsync(
            CancellationToken cancellationToken)
        {
            var findOptions = new FindOptions<Tag>();

            IAsyncCursor<Tag> result = await _mongoCollection.FindAsync<Tag>(
                Builders<Tag>.Filter.Empty, findOptions, cancellationToken);

            return await result.ToListAsync();
        }

        public async Task TryAddTagsAsync(
            IEnumerable<string> tags, CancellationToken cancellationToken)
        {
            var bulkWriteOptions = new BulkWriteOptions();

            IEnumerable<UpdateOneModel<Tag>> bulkWrites = 
                tags.Select(tag => new UpdateOneModel<Tag>(
                    Builders<Tag>.Filter.Eq(t => t.Name, tag), 
                    Builders<Tag>.Update.Inc<int>(t => t.Count, 1))
                    { IsUpsert = true });

            await _mongoCollection
                .BulkWriteAsync(bulkWrites, bulkWriteOptions, cancellationToken);
        }
    }
}
