using System;
using System.Threading;
using System.Threading.Tasks;
using Models;
using MongoDB.Driver;

namespace DataAccess
{
    public class BlogRepository
    {
        private IMongoCollection<Blog> _mongoCollection;

        public BlogRepository(SimpleBlogDbContext simpleBlogDbContext)
        {
            if (simpleBlogDbContext == null)
                throw new ArgumentNullException(nameof(simpleBlogDbContext));

            _mongoCollection = simpleBlogDbContext.CreateCollection<Blog>();
        }

        public async Task AddBlogAsync(
            Blog blog, CancellationToken cancellationToken)
        {
            var insertOneOptions = new InsertOneOptions()
            {
                BypassDocumentValidation = false
            };

            await _mongoCollection
                .InsertOneAsync(blog, insertOneOptions, cancellationToken);
        }
    }
}
