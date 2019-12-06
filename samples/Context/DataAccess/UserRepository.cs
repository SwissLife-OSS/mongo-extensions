using System;
using System.Threading;
using System.Threading.Tasks;
using Models;
using MongoDB.Driver;

namespace SimpleBlog.DataAccess
{
    public class UserRepository : IUserRepository
    {
        private InsertOneOptions _insertOneOptions;
        private IMongoCollection<User> _mongoCollection;

        public UserRepository(ISimpleBlogDbContext simpleBlogDbContext)
        {
            if (simpleBlogDbContext == null)
                throw new ArgumentNullException(nameof(simpleBlogDbContext));

            _mongoCollection = simpleBlogDbContext.CreateCollection<User>();

            _insertOneOptions = new InsertOneOptions()
            {
                BypassDocumentValidation = false
            };
        }

        public async Task AddUserAsync(
            User user, CancellationToken cancellationToken)
        {
            await _mongoCollection
                .InsertOneAsync(user, _insertOneOptions, cancellationToken);
        }

        public async Task AttachBlogToUserAsync(
            string userId, Guid blogId, CancellationToken cancellationToken = default)
        {
            //await _mongoCollection.UpdateOneAsync()
        }
    }
}
