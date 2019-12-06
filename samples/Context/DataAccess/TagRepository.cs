using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Tag = Models.Tag;

namespace SimpleBlog.DataAccess
{
    public class TagRepository : ITagRepository
    {
        private InsertManyOptions _insertManyOptions;
        private IMongoCollection<Tag> _mongoCollection;

        public TagRepository(ISimpleBlogDbContext simpleBlogDbContext)
        {
            if (simpleBlogDbContext == null)
                throw new ArgumentNullException(nameof(simpleBlogDbContext));

            _mongoCollection = simpleBlogDbContext.CreateCollection<Tag>();

            _insertManyOptions = new InsertManyOptions()
            {
                BypassDocumentValidation = false,
                IsOrdered = false
            };
        }

        public async Task TryAddTagsAsync(
            IEnumerable<Tag> tags, CancellationToken cancellationToken)
        {
            //FilterDefinition<Tag> filter = Builders<Tag>.Filter
            //    .Eq(t => t.Name, tenantName);

            //Builders<Tag>.Update.SetOnInsert()

            //await _mongoCollection
            //    .UpdateManyAsync(tags, _insertManyOptions, cancellationToken);
        }
    }
}
