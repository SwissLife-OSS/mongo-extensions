using System;
using System.Threading;
using System.Threading.Tasks;
using Models;
using MongoDB.Driver;

namespace DataAccess
{
    public class UserRepository
    {
        private InsertOneOptions _insertOneOptions;
        private IMongoCollection<User> _mongoCollection;
        
        public UserRepository(SimpleBlogDbContext simpleBlogDbContext)
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
    }
}
