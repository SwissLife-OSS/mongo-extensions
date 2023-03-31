using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Driver;
using MongoDB.Extensions.Context;
using Snapshooter.Xunit;
using Squadron;
using MongoDB.Extensions.Outbox.Core;
using MongoDB.Extensions.Outbox.Persistence;
using Xunit;

namespace MongoDB.Extensions.Outbox.Tests.Persistence
{
    public class MongoDBLockProviderTests : IClassFixture<MongoReplicaSetResource>
    {
        private readonly OutboxDbContext _dbContext;
        private readonly IMongoCollection<MessageLock> _collection;

        public MongoDBLockProviderTests(MongoReplicaSetResource mongoRsResource)
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
                new MessageOptions { LockDuration = TimeSpan.FromSeconds(15) });
            _dbContext.Initialize();
            _collection = _dbContext.CreateCollection<MessageLock>();
        }

        [Fact]
        public async Task AcquireMessageLockAsync_WithoutExistingLockInDb_ShouldAcquireLock()
        {
            //Arrange
            Guid messageId = Guid.NewGuid();
            CancellationToken token = new CancellationTokenSource().Token;

            var provider = new MongoDBLockProvider(_dbContext);

            //Act
            bool acquired = await provider.AcquireMessageLockAsync(messageId, token);

            //Assert
            acquired.Should().BeTrue();
            MessageLock createdLock
                = await _collection.Find(Builders<MessageLock>.Filter.Empty).SingleAsync();
            createdLock.Id.Should().Be(messageId);
            createdLock.LockedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task AcquireMessageLockAsync_WithExistingLockInDb_ShouldNotAcquireLock()
        {
            //Arrange
            Guid messageId = Guid.NewGuid();
            CancellationToken token = new CancellationTokenSource().Token;
            var existingLock = new MessageLock(messageId, DateTime.UtcNow);
            await _collection.InsertOneAsync(existingLock);

            var provider = new MongoDBLockProvider(_dbContext);

            //Act
            bool acquired = await provider.AcquireMessageLockAsync(messageId, token);

            //Assert
            acquired.Should().BeFalse();
            MessageLock createdLock
                = await _collection.Find(Builders<MessageLock>.Filter.Empty).SingleAsync();
            createdLock.Id.Should().Be(existingLock.Id);
            createdLock.LockedAt.Should().BeCloseTo(existingLock.LockedAt, TimeSpan.FromMilliseconds(1));
        }

        [Fact]
        public async Task Ctor_WithDbContext_ShouldGetLockCollectionWithTTLIndex()
        {
            //Arrange
            CancellationToken token = new CancellationTokenSource().Token;

            //Act
            IReadOnlyList<global::MongoDB.Bson.BsonDocument> indexes
                = (await _collection.Indexes.ListAsync(token)).ToList();

            //Assert
            indexes.Should().Contain(
                i => i["key"].AsBsonDocument.Contains("LockedAt")
                && i["key"]["LockedAt"] == 1
                && i["name"] == "LockedAt_1"
                && i["expireAfterSeconds"] == 15);
        }
    }
}
