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
            .AddMongoSessionProvider<TestDbContext>()
            .BuildServiceProvider();
    }

    [Fact]
    public async Task BeginTransactionAsync_ShouldBeginTransaction()
    {
        // Arrange
        ISessionProvider<TestDbContext> sessionProvider = _serviceProvider
            .GetRequiredService<ISessionProvider<TestDbContext>>();

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
        ISessionProvider<TestDbContext> sessionProvider = _serviceProvider
            .GetRequiredService<ISessionProvider<TestDbContext>>();

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
    public async Task MongoSession_Dispose_ShouldDisposeSession()
    {
        // Arrange
        ISessionProvider<TestDbContext> sessionProvider = _serviceProvider
            .GetRequiredService<ISessionProvider<TestDbContext>>();

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
        ISessionProvider<TestDbContext> sessionProvider = _serviceProvider
            .GetRequiredService<ISessionProvider<TestDbContext>>();

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
        ISessionProvider<TestDbContext> sessionProvider = _serviceProvider
            .GetRequiredService<ISessionProvider<TestDbContext>>();

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
}
