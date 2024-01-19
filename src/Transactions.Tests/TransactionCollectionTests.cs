using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using MongoDB.Driver;
using Squadron;
using Xunit;
using static System.Transactions.TransactionScopeAsyncFlowOption;
using static System.Transactions.TransactionScopeOption;

namespace MongoDB.Extensions.Transactions.Tests;

public class TransactionCollectionTests : IClassFixture<MongoReplicaSetResource>
{
    private readonly MongoReplicaSetResource _mongoResource;

    public TransactionCollectionTests(MongoReplicaSetResource mongoResource)
    {
        _mongoResource = mongoResource;
    }

    [Fact]
    public async Task Transaction_SuccessFull()
    {
        // arrange
        IMongoDatabase database = _mongoResource.CreateDatabase();
        IMongoCollection<User> collection =
            database.GetCollection<User>("users").AsTransactionCollection();
        var id = Guid.NewGuid();


        // act
        using (var scope = new TransactionScope(Enabled))
        {
            User user = new(id, "Foo");
            await collection.InsertOneAsync(user);

            scope.Complete();
        }

        // assert
        Assert.Single(await collection.Find(x => x.Id == id).ToListAsync());
    }

    [Fact]
    public async Task Transaction_Fails_WithException()
    {
        // arrange
        IMongoDatabase database = _mongoResource.CreateDatabase();
        IMongoCollection<User> collection =
            database.GetCollection<User>("users").AsTransactionCollection();
        var id = Guid.NewGuid();

        // act
        try
        {
            using (new TransactionScope(Enabled))
            {
                User user = new(id, "Foo");
                await collection.InsertOneAsync(user);
                throw new InvalidOperationException();
            }
        }
        catch
        {
            // ignored
        }

        // assert
        Assert.Empty(await collection.Find(x => x.Id == id).ToListAsync());
    }

    [Fact]
    public async Task Transaction_Fails_WithoutCommit()
    {
        // arrange
        IMongoDatabase database = _mongoResource.CreateDatabase();
        IMongoCollection<User> collection =
            database.GetCollection<User>("users").AsTransactionCollection();
        var id = Guid.NewGuid();

        // act
        try
        {
            using (new TransactionScope(Enabled))
            {
                User user = new(id, "Foo");
                await collection.InsertOneAsync(user);
            }
        }
        catch
        {
            // ignored
        }

        // assert
        Assert.Empty(await collection.Find(x => x.Id == id).ToListAsync());
    }

    [Fact]
    public async Task Transaction_Should_ReadChangesDuringTransaction()
    {
        // arrange
        IMongoDatabase database = _mongoResource.CreateDatabase();
        IMongoCollection<User> collection =
            database.GetCollection<User>("users").AsTransactionCollection();
        var id = Guid.NewGuid();
        IReadOnlyList<User> users = Array.Empty<User>();

        // act
        using (new TransactionScope(Enabled))
        {
            User user = new User(id, "Foo3");
            await collection.InsertOneAsync(user);
            users = await collection.Find(FilterDefinition<User>.Empty).ToListAsync();
        }

        // assert
        Assert.Single(users);
        Assert.Empty(await collection.Find(FilterDefinition<User>.Empty).ToListAsync());
    }

    [Fact]
    public async Task NestedTransaction_Should_Succeed()
    {
        // arrange
        IMongoDatabase database = _mongoResource.CreateDatabase();
        IMongoCollection<User> collection =
            database.GetCollection<User>("users").AsTransactionCollection();

        // act
        using (var scope = new TransactionScope(Enabled))
        {
            User user1 = new(Guid.NewGuid(), "Foo1");
            await collection.InsertOneAsync(user1);

            using (var innerScope =
                new TransactionScope(Enabled))
            {
                User user2 = new(Guid.NewGuid(), "Foo2");
                await collection.InsertOneAsync(user2);
                innerScope.Complete();
            }

            scope.Complete();
        }

        // assert
        List<User> dbUsers = await collection.Find(FilterDefinition<User>.Empty).ToListAsync();
        Assert.Equal(2, dbUsers.Count);
    }

    [Fact]
    public async Task NestedTransaction_InnerTransactionNotCommitted()
    {
        // arrange
        IMongoDatabase database = _mongoResource.CreateDatabase();
        IMongoCollection<User> collection =
            database.GetCollection<User>("users").AsTransactionCollection();

        // act
        TransactionAbortedException ex = await Assert.ThrowsAsync<TransactionAbortedException>(
            async () =>
            {
                using (var scope = new TransactionScope(Enabled))
                {
                    User user1 = new(Guid.NewGuid(), "Foo1");
                    await collection.InsertOneAsync(user1);

                    using (var innerScope =
                        new TransactionScope(Enabled))
                    {
                        User user2 =
                            new(Guid.NewGuid(), "Foo2");
                        await collection.InsertOneAsync(user2);
                    }

                    scope.Complete();
                }
            });

        // assert
        Assert.Equal("The transaction has aborted.", ex.Message);
        Assert.Empty(await collection.Find(FilterDefinition<User>.Empty).ToListAsync());
    }

    [Fact]
    public async Task NestedTransaction_InnerThrowsException()
    {
        // arrange
        IMongoDatabase database = _mongoResource.CreateDatabase();
        IMongoCollection<User> collection =
            database.GetCollection<User>("users").AsTransactionCollection();

        // act
        TransactionAbortedException ex = await Assert.ThrowsAsync<TransactionAbortedException>(
            async () =>
            {
                using (var scope = new TransactionScope(Enabled))
                {
                    User user1 = new(Guid.NewGuid(), "Foo1");
                    await collection.InsertOneAsync(user1);

                    try
                    {
                        using (var innerScope = new TransactionScope(Enabled))
                        {
                            User user2 =
                                new(Guid.NewGuid(), "Foo2");
                            await collection.InsertOneAsync(user2);
                            throw new Exception();
                        }
                    }
                    catch
                    {
                    }

                    scope.Complete();
                }
            });

        // assert
        Assert.Equal("The transaction has aborted.", ex.Message);
        Assert.Empty(await collection.Find(FilterDefinition<User>.Empty).ToListAsync());
    }

    [Fact]
    public async Task NestedTransaction_RequiresNew_Should_Succeed()
    {
        // arrange
        IMongoDatabase database = _mongoResource.CreateDatabase();
        IMongoCollection<User> collection =
            database.GetCollection<User>("users").AsTransactionCollection();
        await collection
            .InsertOneAsync(new User(Guid.NewGuid(), "Foo2"));

        // act
        using (var scope = new TransactionScope(RequiresNew, Enabled))
        {
            var user1 = new User(Guid.NewGuid(), "Foo1");
            await collection.InsertOneAsync(user1);

            using (var innerScope = new TransactionScope(RequiresNew, Enabled))
            {
                var user2 = new User(Guid.NewGuid(), "Foo2");
                await collection.InsertOneAsync(user2);
                innerScope.Complete();
            }

            scope.Complete();
        }

        // assert
        List<User> dbUsers = await collection.Find(FilterDefinition<User>.Empty).ToListAsync();
        Assert.Equal(3, dbUsers.Count);
    }

    [Fact]
    public async Task NestedTransaction_RequiresNew_InnerTransactionNotCommitted()
    {
        // arrange
        IMongoDatabase database = _mongoResource.CreateDatabase();
        IMongoCollection<User> collection =
            database.GetCollection<User>("users").AsTransactionCollection();

        // act
        using (var scope = new TransactionScope(RequiresNew, Enabled))
        {
            var user1 = new User(Guid.NewGuid(), "Foo1");
            await collection.InsertOneAsync(user1);

            using (var innerScope = new TransactionScope(RequiresNew, Enabled))
            {
                var user2 = new User(Guid.NewGuid(), "Foo2");
                await collection.InsertOneAsync(user2);
            }

            scope.Complete();
        }

        // assert
        Assert.Single(await collection.Find(FilterDefinition<User>.Empty).ToListAsync());
    }

    [Fact]
    public async Task NestedTransaction_RequiresNew_InnerThrowsException()
    {
        // arrange
        IMongoDatabase database = _mongoResource.CreateDatabase();
        IMongoCollection<User> collection =
            database.GetCollection<User>("users").AsTransactionCollection();

        // act
        using (var scope = new TransactionScope(RequiresNew, Enabled))
        {
            var user1 = new User(Guid.NewGuid(), "Foo1");
            await collection.InsertOneAsync(user1);

            try
            {
                using (var innerScope = new TransactionScope(RequiresNew, Enabled))
                {
                    var user2 = new User(Guid.NewGuid(), "Foo2");
                    await collection.InsertOneAsync(user2);
                    throw new Exception();
                }
            }
            catch
            {
            }

            scope.Complete();
        }

        // assert
        Assert.Single(await collection.Find(FilterDefinition<User>.Empty).ToListAsync());
    }

    [Fact]
    public async Task NestedTransaction_Suppress_Should_Succeed()
    {
        // arrange
        IMongoDatabase database = _mongoResource.CreateDatabase();
        IMongoCollection<User> collection =
            database.GetCollection<User>("users").AsTransactionCollection();

        // act
        using (var scope = new TransactionScope(TransactionScopeOption.Suppress, Enabled))
        {
            var user1 = new User(Guid.NewGuid(), "Foo1");
            await collection.InsertOneAsync(user1);

            using (var innerScope = new TransactionScope(Enabled))
            {
                var user2 = new User(Guid.NewGuid(), "Foo2");
                await collection.InsertOneAsync(user2);
                innerScope.Complete();
            }

            scope.Complete();
        }

        // assert
        List<User> dbUsers = await collection.Find(FilterDefinition<User>.Empty).ToListAsync();
        Assert.Equal(2, dbUsers.Count);
    }

    [Fact]
    public async Task NestedTransaction_Suppress_InnerTransactionNotCommitted()
    {
        // arrange
        IMongoDatabase database = _mongoResource.CreateDatabase();
        IMongoCollection<User> collection =
            database.GetCollection<User>("users").AsTransactionCollection();

        // act
        using (var scope =
            new TransactionScope(TransactionScopeOption.Suppress, Enabled))
        {
            var user1 = new User(Guid.NewGuid(), "Foo1");
            await collection.InsertOneAsync(user1);

            using (var innerScope =
                new TransactionScope(TransactionScopeOption.Suppress, Enabled))
            {
                var user2 = new User(Guid.NewGuid(), "Foo2");
                await collection.InsertOneAsync(user2);
            }

            scope.Complete();
        }

        // assert
        List<User> users = await collection.Find(FilterDefinition<User>.Empty).ToListAsync();
        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task NestedTransaction_Suppress_InnerThrowsException()
    {
        // arrange
        IMongoDatabase database = _mongoResource.CreateDatabase();
        IMongoCollection<User> collection =
            database.GetCollection<User>("users").AsTransactionCollection();

        // act
        using (var scope =
            new TransactionScope(TransactionScopeOption.Suppress, Enabled))
        {
            var user1 = new User(Guid.NewGuid(), "Foo1");
            await collection.InsertOneAsync(user1);

            try
            {
                using (var innerScope =
                    new TransactionScope(TransactionScopeOption.Suppress, Enabled))
                {
                    var user2 = new User(Guid.NewGuid(), "Foo2");
                    await collection.InsertOneAsync(user2);
                    throw new Exception();
                }
            }
            catch
            {
            }

            scope.Complete();
        }

        // assert
        List<User> users = await collection.Find(FilterDefinition<User>.Empty).ToListAsync();
        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task Transaction_SuccessFull_Concurrent()
    {
        // arrange
        IMongoDatabase database = _mongoResource.CreateDatabase();
        IMongoCollection<User> collection =
            database.GetCollection<User>("users").AsTransactionCollection();
        var tasks = new List<Task>();
        const int taskCount = 10;
        const int documentCount = 10;
        await collection.InsertOneAsync(
            new User(Guid.NewGuid(), "Foo"));

        // act
        for (var i = 0; i < taskCount; i++)
        {
            async Task task()
            {
                for (var j = 0; j < documentCount; j++)
                {
                    using (var scope = new TransactionScope(Enabled))
                    {
                        await collection.InsertOneAsync(
                            new User(Guid.NewGuid(), "Foo"));

                        scope.Complete();
                    }
                }
            }

            tasks.Add(task());
        }

        await Task.WhenAll(tasks);

        // assert
        Assert.Equal(101, await collection.CountDocumentsAsync(FilterDefinition<User>.Empty));
    }
}
