using System;
using System.Threading;
using System.Threading.Tasks;
using Models;
using MongoDB.Driver;

namespace SimpleBlog.DataAccess
{
    public class BlogRepository : IBlogRepository
    {
        private InsertOneOptions _insertOneOptions;
        private IMongoCollection<Blog> _mongoCollection;

        public BlogRepository(ISimpleBlogDbContext simpleBlogDbContext)
        {
            if (simpleBlogDbContext == null)
                throw new ArgumentNullException(nameof(simpleBlogDbContext));

            _mongoCollection = simpleBlogDbContext.CreateCollection<Blog>();

            _insertOneOptions = new InsertOneOptions()
            {
                BypassDocumentValidation = false
            };
        }

        public async Task AddBlogAsync(
            Blog blog, CancellationToken cancellationToken)
        {
            await _mongoCollection
                .InsertOneAsync(blog, _insertOneOptions, cancellationToken);
        }
    }
}
