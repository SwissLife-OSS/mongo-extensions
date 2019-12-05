using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Tag = Models.Tag;

namespace DataAccess
{
    public class TagRepository
    {
        private InsertOneOptions _insertOneOptions;
        private IMongoCollection<Tag> _mongoCollection;
        
        public TagRepository(SimpleBlogDbContext simpleBlogDbContext)
        {
            if (simpleBlogDbContext == null)
                throw new ArgumentNullException(nameof(simpleBlogDbContext));

            _mongoCollection = simpleBlogDbContext.CreateCollection<Tag>();

            _insertOneOptions = new InsertOneOptions()
            {
                BypassDocumentValidation = false
            };
        }
        
        public async Task AddTagAsync(
            Tag tag, CancellationToken cancellationToken)
        {
            await _mongoCollection
                .InsertOneAsync(tag, _insertOneOptions, cancellationToken);
        }
    }
}
