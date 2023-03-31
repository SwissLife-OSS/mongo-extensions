using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Driver;
using MongoDB.Extensions.Context;
using Squadron;
using SwissLife.MongoDB.Extensions.Outbox.Core;
using SwissLife.MongoDB.Extensions.Outbox.Persistence;
using Xunit;

namespace SwissLife.MongoDB.Extensions.Outbox.Tests.Persistence
{
    public class OutboxRepositoryTests : IClassFixture<MongoReplicaSetResource>
    {
        private readonly OutboxDbContext _dbContext;
        private readonly MongoSessionProvider _sessionProvider;
        private readonly IMongoCollection<OutboxMessage> _collection;

        public OutboxRepositoryTests(MongoReplicaSetResource mongoRsResource)
        {
            IMongoDatabase mongoDatabase = mongoRsResource.CreateDatabase();
            mongoRsResource.Client.DisableTableScan();

            var mongoOptions = new MongoOptions
            {
                ConnectionString = mongoRsResource.ConnectionString,
                DatabaseName = mongoDatabase.DatabaseNamespace.DatabaseName
            };
            _dbContext = new OutboxDbContext(
                mongoOptions,
                new MessageOptions { LockDuration = TimeSpan.FromSeconds(30) });
            _dbContext.Initialize();

            _sessionProvider = new MongoSessionProvider(_dbContext);

            _collection = _dbContext.CreateCollection<OutboxMessage>();
        }

        [Fact]
        public async Task GetPendingMessagesAsync_WithMessagesOlderThanMinimumAge_ShouldReturnTheseMessages()
        {
            //Arrange testData
            var message = new OutboxMessage(
                id: Guid.NewGuid(),
                payload: new DummyPayload(Guid.NewGuid()),
                creationDate: DateTime.UtcNow.AddSeconds(-31)
            );
            await _collection.InsertOneAsync(message, new InsertOneOptions(), CancellationToken.None);

            //Arrange dependencies
            var options = new OutboxOptions
            {
                FallbackWorker = new OutboxFallBackWorkerOptions
                {
                    MessageMinimumAge = TimeSpan.FromSeconds(30)
                }
            };

            var repository = new OutboxRepository(options, _dbContext, _sessionProvider);

            //Act
            IReadOnlyList<OutboxMessage> messages
                = await repository.GetPendingMessagesAsync(CancellationToken.None);

            //Assert
            OutboxMessage foundMessage = messages.Should().ContainSingle().Subject;
            foundMessage.Id.Should().Be(message.Id);
            DummyPayload foundPayload = foundMessage.Payload.Should().BeOfType<DummyPayload>().Subject;
            foundPayload.Should().BeEquivalentTo(message.Payload as DummyPayload);
        }

        [Fact]
        public async Task GetPendingMessagesAsync_WithMessagesYoungerThanMinimumAge_ShouldNotReturnTheseMessages()
        {
            //Arrange testData
            var message = new OutboxMessage(
                id: Guid.NewGuid(),
                payload: new DummyPayload(Guid.NewGuid()),
                creationDate: DateTime.UtcNow
            );
            await _collection.InsertOneAsync(message, new InsertOneOptions(), CancellationToken.None);

            //Arrange dependencies
            var options = new OutboxOptions
            {
                FallbackWorker = new OutboxFallBackWorkerOptions
                {
                    MessageMinimumAge = TimeSpan.FromSeconds(30)
                }
            };

            var repository = new OutboxRepository(options, _dbContext, _sessionProvider);

            //Act
            IReadOnlyList<OutboxMessage> messages
                = await repository.GetPendingMessagesAsync(CancellationToken.None);

            //Assert
            messages.Should().BeEmpty();
        }

        [Fact]
        public async Task BeginTransactionAsync_ShouldAlwaysReturnMongoSessionWithOpenTransaction()
        {
            //Arrange
            var options = new OutboxOptions
            {
                FallbackWorker = new OutboxFallBackWorkerOptions
                {
                    MessageMinimumAge = TimeSpan.FromSeconds(30)
                }
            };

            var repository = new OutboxRepository(options, _dbContext, _sessionProvider);

            //Act
            ITransactionSession transactionSession
                = await repository.BeginTransactionAsync(CancellationToken.None);

            //Assert
            MongoTransactionSession openSession
                = transactionSession.Should().BeOfType<MongoTransactionSession>().Subject;
            openSession.Session.IsInTransaction.Should().BeTrue();
        }
    }

    public class DummyPayload
    {
        public DummyPayload(Guid dummyValue)
        {
            DummyValue = dummyValue;
        }

        public Guid DummyValue { get; set; }
    }
}
