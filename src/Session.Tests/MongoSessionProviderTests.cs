using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Extensions.Context;
using Squadron;
using Xunit;

namespace MongoDB.Extensions.Session.Tests;

public class MongoSessionProviderTests : IClassFixture<MongoReplicaSetResource>
{
    private readonly IServiceProvider _serviceProvider;

    public MongoSessionProviderTests(MongoReplicaSetResource mongoResource)
    {
        var mongoOptions = new MongoOptions
        {
            ConnectionString = mongoResource.ConnectionString,
            DatabaseName = mongoResource.CreateDatabase().DatabaseNamespace.DatabaseName
        };

        _serviceProvider = new ServiceCollection()
            .AddSingleton(mongoOptions)
            .AddMongoSessionProvider<TestDbContext, ITestScope>()
            .BuildServiceProvider();
    }

    [Fact]
    public async Task BeginTransactionAsync_ShouldBeginTransaction()
    {
        // Arrange
        ISessionProvider<ITestScope> sessionProvider = _serviceProvider
            .GetRequiredService<ISessionProvider<ITestScope>>();

        // Act
        ITransactionSession transactionSession = await sessionProvider
            .BeginTransactionAsync(CancellationToken.None);

        // Assert
        transactionSession.Should().NotBeNull();
        IClientSessionHandle clientSessionHandle = transactionSession.GetSessionHandle();
        clientSessionHandle.ServerSession.Id["id"].AsGuid.Should().NotBeEmpty();
        clientSessionHandle.IsInTransaction.Should().BeTrue();
    }

    [Fact]
    public async Task StartSessionAsync_ShouldStartSession()
    {
        // Arrange
        ISessionProvider<ITestScope> sessionProvider = _serviceProvider
            .GetRequiredService<ISessionProvider<ITestScope>>();

        // Act
        ISession session = await sessionProvider
            .StartSessionAsync(CancellationToken.None);

        // Assert
        session.Should().NotBeNull();
        IClientSessionHandle clientSessionHandle = session.GetSessionHandle();
        clientSessionHandle.ServerSession.Id["id"].AsGuid.Should().NotBeEmpty();
        clientSessionHandle.IsInTransaction.Should().BeFalse();
    }

    [Fact]
    public async Task StartTransaction_ShouldBeginTransaction()
    {
        // Arrange
        var dbContext = new TestDbContext(_mongoOptions);
        ITestSessionProvider sessionProvider = new TestSessionProvider(dbContext);
        ISession session = await sessionProvider
            .StartSessionAsync(CancellationToken.None);

        // Act
        ITransactionSession transactionSession = session
            .StartTransaction(CancellationToken.None);

        // Assert
        transactionSession.Should().NotBeNull();
        IClientSessionHandle clientSessionHandle = transactionSession.GetSessionHandle();
        clientSessionHandle.ServerSession.Id["id"].AsGuid.Should().NotBeEmpty();
        clientSessionHandle.IsInTransaction.Should().BeTrue();
    }

    [Fact]
    public async Task MongoSession_Dispose_ShouldDisposeSession()
    {
        // Arrange
        ISessionProvider<ITestScope> sessionProvider = _serviceProvider
            .GetRequiredService<ISessionProvider<ITestScope>>();

        ISession session = await sessionProvider
            .StartSessionAsync(CancellationToken.None);

        // Act
        session.Dispose();

        // Assert
        IClientSessionHandle clientSessionHandle = session.GetSessionHandle();
        Assert.Throws<ObjectDisposedException>(() => clientSessionHandle.ServerSession);
    }

    [Fact]
    public async Task MongoTransactionSession_Dispose_ShouldDisposeSession()
    {
        // Arrange
        ISessionProvider<ITestScope> sessionProvider = _serviceProvider
            .GetRequiredService<ISessionProvider<ITestScope>>();

        ITransactionSession transactionSession = await sessionProvider
            .BeginTransactionAsync(CancellationToken.None);

        // Act
        transactionSession.Dispose();

        // Assert
        IClientSessionHandle clientSessionHandle = transactionSession.GetSessionHandle();
        Assert.Throws<ObjectDisposedException>(() => clientSessionHandle.ServerSession);
    }

    [Fact]
    public async Task MongoTransactionSession_NotCommitting_ShouldNotAffectDatabase()
    {
        // Arrange
        ISessionProvider<ITestScope> sessionProvider = _serviceProvider
            .GetRequiredService<ISessionProvider<ITestScope>>();

        ITransactionSession transactionSession = await sessionProvider
            .BeginTransactionAsync(CancellationToken.None);
        TestDbContext context = _serviceProvider.GetRequiredService<TestDbContext>();
        IMongoCollection<BsonDocument> collection = context.CreateCollection<BsonDocument>();
        await collection.InsertOneAsync(transactionSession.GetSessionHandle(), new BsonDocument());

        // Act
        // Not committing the transaction

        // Assert
        (await collection
            .Find(FilterDefinition<BsonDocument>.Empty)
            .ToListAsync())
            .Count.Should().Be(0);
    }

    private class TestDbContext : MongoDbContext
    {
        public TestDbContext(MongoOptions mongoOptions)
            : base(mongoOptions)
        {
        }

        protected override void OnConfiguring(IMongoDatabaseBuilder mongoDatabaseBuilder)
        {
        }
    }

    private interface ITestScope
    {
    }
}
