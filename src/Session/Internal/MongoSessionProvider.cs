using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Extensions.Context;

namespace MongoDB.Extensions.Session;

public class MongoSessionProvider<TContext, TScope> : ISessionProvider<TScope>
    where TContext : IMongoDbContext
{
    private readonly IMongoClient _mongoClient;

    public MongoSessionProvider(TContext context)
    {
        _mongoClient = context.Client;
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

    public async Task<ISession> StartSessionAsync(
        CancellationToken cancellationToken)
    {
        IClientSessionHandle clientSession = await _mongoClient
            .StartSessionAsync(cancellationToken: cancellationToken);

        return new MongoSession(clientSession);
    }
}
