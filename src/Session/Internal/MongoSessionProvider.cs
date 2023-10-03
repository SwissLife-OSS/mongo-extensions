using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Extensions.Context;

namespace MongoDB.Extensions.Session;

public class MongoSessionProvider<TContext> : ISessionProvider
    where TContext : IMongoDbContext
{
    private readonly IMongoClient _mongoClient;

    public MongoSessionProvider(TContext context)
    {
        _mongoClient = context.Client;
    }

    protected virtual TransactionOptions TransactionOptions { get; } = new(
        ReadConcern.Majority,
        ReadPreference.Primary,
        WriteConcern.WMajority.With(journal: true),
        TimeSpan.FromSeconds(180));

    public async Task<ITransactionSession> BeginTransactionAsync(
        CancellationToken cancellationToken)
    {
        IClientSessionHandle clientSession = await _mongoClient
            .StartSessionAsync(cancellationToken: cancellationToken);

        clientSession.StartTransaction(TransactionOptions);

        return new MongoTransactionSession(clientSession, cancellationToken);
    }

    public async Task<ISession> StartSessionAsync(
        CancellationToken cancellationToken)
    {
        IClientSessionHandle clientSession = await _mongoClient
            .StartSessionAsync(cancellationToken: cancellationToken);

        return new MongoSession(clientSession, TransactionOptions);
    }
}
