using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Extensions.Context;

namespace MongoDB.Extensions.Session;

public class MongoSessionProvider<TContext, TScope> : MongoSessionProvider<TScope>
    where TContext : IMongoDbContext
{
    public MongoSessionProvider(TContext context) 
        : base(new MongoClientSessionFactory(context.Client))
    {
    }
}

public class MongoSessionProvider<TScope> : ISessionProvider<TScope>
{
    private readonly ISessionFactory _sessionFactory;

    protected MongoSessionProvider(ISessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    protected virtual TransactionOptions TransactionOptions { get; } = new(
        ReadConcern.Majority,
        ReadPreference.Primary,
        WriteConcern.WMajority.With(journal: true),
        TimeSpan.FromSeconds(180));

    public async Task<ITransactionSession> BeginTransactionAsync(
        CancellationToken cancellationToken)
    {
        IClientSessionHandle clientSession = await _sessionFactory
            .CreateSessionAsync(cancellationToken);

        clientSession.StartTransaction(TransactionOptions);

        return new MongoTransactionSession(clientSession, cancellationToken);
    }

    public async Task<ISession> StartSessionAsync(
        CancellationToken cancellationToken)
    {
        IClientSessionHandle clientSession = await _sessionFactory
            .CreateSessionAsync(cancellationToken);

        return new MongoSession(clientSession, TransactionOptions);
    }
}