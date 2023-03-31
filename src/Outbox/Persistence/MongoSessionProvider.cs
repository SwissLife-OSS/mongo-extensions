using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Extensions.Outbox.Core;

namespace MongoDB.Extensions.Outbox.Persistence
{
    public class MongoSessionProvider : ISessionProvider
    {
        private readonly IMongoClient _mongoClient;

        public MongoSessionProvider(IOutboxDbContext dbContext)
        {
            _mongoClient = dbContext.Client;
        }

        public Task<ITransactionSession> BeginTransactionAsync(
            CancellationToken cancellationToken)
        {
            return BeginTransactionAsync(true, cancellationToken);
        }

        private async Task<ITransactionSession> BeginTransactionAsync(
            bool safeModeEnabled,
            CancellationToken cancellationToken)
        {
            IClientSessionHandle clientSession = await _mongoClient
                .StartSessionAsync(cancellationToken: cancellationToken);

            var transactionOptions = new TransactionOptions(
                ReadConcern.Majority,
                ReadPreference.Primary,
                WriteConcern.WMajority.With(journal: safeModeEnabled),
                TimeSpan.FromSeconds(180));

            clientSession.StartTransaction(transactionOptions);

            return new MongoTransactionSession(clientSession, cancellationToken);
        }
    }
}
