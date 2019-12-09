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

        public async Task<User> GetUserAsync(
            string userId, CancellationToken cancellationToken= default)
        {
            FilterDefinition<User> filter = Builders<User>.Filter
                .Eq<string>(user => user.UserId, userId);
            
            return await _mongoCollection
                .Find<User>(filter)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task AddUserAsync(
            User user, CancellationToken cancellationToken = default)
        {
            await _mongoCollection
                .InsertOneAsync(user, _insertOneOptions, cancellationToken);
        }

        public async Task AttachBlogToUserAsync(
            string userId, Guid blogId, CancellationToken cancellationToken = default)
        {
            UpdateDefinition<User> update = Builders<User>.Update
                .AddToSet(u => u.Posts, blogId);

            var updateOptions = new UpdateOptions()
            {
                IsUpsert = true
            };

            await _mongoCollection.UpdateOneAsync<User>(
                user => user.UserId == userId, update, updateOptions, cancellationToken);
        }
    }
}
