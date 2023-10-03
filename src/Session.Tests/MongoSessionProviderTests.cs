using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Extensions.Context;
using Squadron;
using Xunit;

namespace MongoDB.Extensions.Session.Tests;

public class MongoSessionProviderTests : IClassFixture<MongoReplicaSetResource>
{
    private readonly MongoOptions _mongoOptions;

    public MongoSessionProviderTests(MongoReplicaSetResource mongoResource)
    {
        _mongoOptions = new MongoOptions
        {
            ConnectionString = mongoResource.ConnectionString,
            DatabaseName = mongoResource.CreateDatabase().DatabaseNamespace.DatabaseName
        };
    }

    [Fact]
    public async Task BeginTransactionAsync_ShouldBeginTransaction()
    {
        // Arrange
        var dbContext = new TestDbContext(_mongoOptions);
        ITestSessionProvider sessionProvider = new TestSessionProvider(dbContext);

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
        var dbContext = new TestDbContext(_mongoOptions);
        ITestSessionProvider sessionProvider = new TestSessionProvider(dbContext);

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
        var dbContext = new TestDbContext(_mongoOptions);
        ITestSessionProvider sessionProvider = new TestSessionProvider(dbContext);

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
        var dbContext = new TestDbContext(_mongoOptions);
        ITestSessionProvider sessionProvider = new TestSessionProvider(dbContext);

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
        var dbContext = new TestDbContext(_mongoOptions);
        ITestSessionProvider sessionProvider = new TestSessionProvider(dbContext);

        ITransactionSession transactionSession = await sessionProvider
            .BeginTransactionAsync(CancellationToken.None);
        var collection = dbContext.CreateCollection<BsonDocument>();
        await collection.InsertOneAsync(transactionSession.GetSessionHandle(), new BsonDocument());

        // Act
        // Not committing the transaction

        // Assert
        (await collection
            .Find(FilterDefinition<BsonDocument>.Empty)
            .ToListAsync())
            .Count.Should().Be(0);
    }

    private interface ITestDbContext : IMongoDbContext { }

    private class TestDbContext : MongoDbContext, ITestDbContext
    {
        public TestDbContext(MongoOptions mongoOptions)
            : base(mongoOptions)
        {
        }

        protected override void OnConfiguring(IMongoDatabaseBuilder mongoDatabaseBuilder)
        {
        }
    }

    private interface ITestSessionProvider : ISessionProvider { }

    private class TestSessionProvider : MongoSessionProvider<ITestDbContext>, ITestSessionProvider
    {
        public TestSessionProvider(ITestDbContext context)
            : base(context)
        {
        }
    }
}
