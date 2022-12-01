using System;
using System.Threading.Tasks;
using System.Threading;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context;

public interface IMongoTransactionDbContext : IMongoDbContext, IDisposable
{
    TransactionOptions TransactionOptions { get; }

    IMongoCollection<TDocument> GetCollection<TDocument>() where TDocument : class;

    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
