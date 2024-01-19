using System.Threading.Tasks;
using System.Threading;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context
{
    public interface IMongoDbTransaction
    {
        Task<IMongoTransactionDbContext> StartNewTransactionAsync(
            TransactionOptions? transactionOptions = null,
            CancellationToken cancellationToken = default);
    }
}
