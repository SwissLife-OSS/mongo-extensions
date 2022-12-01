using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Prime.Extensions;
using Snapshooter.Xunit;
using Squadron;
using Xunit;

namespace MongoDB.Extensions.Context.Tests
{
    public class MongoTransactionDbContextTests : IClassFixture<MongoReplicaSetResource>
    {
        private readonly MongoOptions _mongoOptions;
        private readonly IMongoDatabase _mongoDatabase;

        public MongoTransactionDbContextTests(MongoReplicaSetResource mongoResource)
        {
            _mongoDatabase = mongoResource.CreateDatabase();
            _mongoOptions = new MongoOptions
            {
                ConnectionString = mongoResource.ConnectionString,
                DatabaseName = _mongoDatabase.DatabaseNamespace.DatabaseName
            };
        }

        #region StartNewTransactionAsync Tests

        [Fact]
        public async Task StartNewTransactionAsync_CreateNewTransactionDbContext_OptionsCorrect()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);

            // Act
            using IMongoTransactionDbContext transactionDbContext =
                await testMongoDbContext.StartNewTransactionAsync();

            // Assert
            Snapshot.Match(transactionDbContext.TransactionOptions);
        }

        [Fact]
        public async Task StartNewTransactionAsync_SetTransactionTransactionOptions_OptionsCorrect()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);

            TransactionOptions transactionOptions = 
                new TransactionOptions(
                    ReadConcern.Local,
                    ReadPreference.Secondary,
                    WriteConcern.W3.With(journal: false),
                    TimeSpan.FromSeconds(300));

            // Act
            IMongoTransactionDbContext transactionDbContext =
                await testMongoDbContext.StartNewTransactionAsync(transactionOptions);

            // Assert
            Snapshot.Match(transactionDbContext.TransactionOptions);
        }

        [Fact]
        public async Task StartNewTransactionAsync_ClientSessionIdCompare_DifferentIds()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);
            IClientSessionHandle baseSession =
                await testMongoDbContext.Client.StartSessionAsync();
            
            // Act
            using MongoTransactionDbContext transactionDbContext = (MongoTransactionDbContext)
                await testMongoDbContext.StartNewTransactionAsync();

            // Assert
            Assert.NotEqual(
                baseSession.GetSessionId(),
                transactionDbContext.ClientSession.GetSessionId());
        }

        [Fact]
        public async Task StartNewTransactionAsync_GetTwicSameCollections_CollectionCached()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);

            using IMongoTransactionDbContext transactionDbContext =
                await testMongoDbContext.StartNewTransactionAsync();

            // Act
            IMongoCollection<Bar> ref1 = transactionDbContext.CreateCollection<Bar>();
            IMongoCollection<Bar> ref2 = transactionDbContext.CreateCollection<Bar>();
            IMongoCollection<Bar> ref3 = transactionDbContext.GetCollection<Bar>();

            // Assert
            Assert.Same(ref1, ref2);
            Assert.Same(ref2, ref3);
        }

        [Fact]
        public async Task StartNewTransactionAsync_AddFooBarWithoutCommit_NothingSaved()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);

            using IMongoTransactionDbContext transactionDbContext =
                await testMongoDbContext.StartNewTransactionAsync();

            // Act
            transactionDbContext.GetCollection<Foo>().InsertOne(new Foo { Id = 1, FooName = "Foo1" });
            transactionDbContext.GetCollection<Bar>().InsertOne(new Bar { Id = 1, BarName = "Bar1" });

            // Assert
            Assert.Empty(_mongoDatabase.GetCollection<Foo>().Dump());
            Assert.Empty(_mongoDatabase.GetCollection<Bar>().Dump());
        }

        [Fact]
        public async Task StartNewTransactionAsync_AddFooBarWithCommit_AllSaved()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);

            using IMongoTransactionDbContext transactionDbContext =
                await testMongoDbContext.StartNewTransactionAsync();

            // Act
            transactionDbContext.GetCollection<Foo>().InsertOne(new Foo { Id = 1, FooName = "Foo1" });
            transactionDbContext.GetCollection<Bar>().InsertOne(new Bar { Id = 1, BarName = "Bar1" });
            await transactionDbContext.CommitAsync();

            // Assert
            Snapshot.Match(_mongoDatabase.DumpAllCollections());
        }

        [Fact]
        public async Task StartNewTransactionAsync_AddFooBarWithRollback_NothingSaved()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);

            using IMongoTransactionDbContext transactionDbContext =
                await testMongoDbContext.StartNewTransactionAsync();

            // Act
            transactionDbContext.GetCollection<Foo>().InsertOne(new Foo { Id = 1, FooName = "Foo1" });
            transactionDbContext.GetCollection<Bar>().InsertOne(new Bar { Id = 1, BarName = "Bar1" });
            await transactionDbContext.RollbackAsync();

            // Assert
            Assert.Empty(_mongoDatabase.GetCollection<Foo>().Dump());
            Assert.Empty(_mongoDatabase.GetCollection<Bar>().Dump());
        }

        [Fact]
        public async Task StartNewTransactionAsync_TwoTransactionContextWithSameObjects_ConcurrencyExceptionAndSaved()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);

            using IMongoTransactionDbContext transactionDbContext1 =
                await testMongoDbContext.StartNewTransactionAsync();

            using IMongoTransactionDbContext transactionDbContext2 =
                await testMongoDbContext.StartNewTransactionAsync();

            IMongoCollection<Foo> transactionCollection1 =
                transactionDbContext1.GetCollection<Foo>();

            IMongoCollection<Foo>  transactionCollection2 =
                transactionDbContext2.GetCollection<Foo>();

            // Act
            transactionCollection1.InsertOne(new Foo { Id = 1, FooName = "Foo1a" });
            transactionCollection2.InsertOne(new Foo { Id = 1, FooName = "Foo1b" });
            await transactionDbContext1.CommitAsync();
            Func<Task> commit2Action = async () => await transactionDbContext2.CommitAsync();

            // Assert
            await Assert.ThrowsAsync<MongoCommandException>(commit2Action);
            Snapshot.Match(_mongoDatabase.DumpAllCollections());
        }

        [Fact]
        public async Task StartNewTransactionAsync_TwoTransactionContextWithDifferentObjectsNoCommit_NothingSaved()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);
            testMongoDbContext.CreateCollection<Foo>();

            using IMongoTransactionDbContext transactionDbContext1 =
                await testMongoDbContext.StartNewTransactionAsync();

            using IMongoTransactionDbContext transactionDbContext2 =
                await testMongoDbContext.StartNewTransactionAsync();

            IMongoCollection<Foo> transactionCollection1 =
                transactionDbContext1.GetCollection<Foo>();

            IMongoCollection<Foo>  transactionCollection2 =
                transactionDbContext2.GetCollection<Foo>();

            // Act
            transactionCollection1.InsertOne(new Foo { Id = 1, FooName = "Foo1a" });
            transactionCollection2.InsertOne(new Foo { Id = 2, FooName = "Foo1b" });

            // Assert
            Assert.Empty(_mongoDatabase.GetCollection<Foo>().Dump());
            Assert.Empty(_mongoDatabase.GetCollection<Bar>().Dump());
        }

        [Fact]
        public async Task StartNewTransactionAsync_TwoTransactionContextWithDifferentObjects_AllSaved()
        {
            // Arrange
            var testMongoDbContext = new TestMongoDbContext(_mongoOptions);
            testMongoDbContext.CreateCollection<Foo>();

            using IMongoTransactionDbContext transactionDbContext1 =
                await testMongoDbContext.StartNewTransactionAsync();

            using IMongoTransactionDbContext transactionDbContext2 =
                await testMongoDbContext.StartNewTransactionAsync();

            IMongoCollection<Foo> transactionCollection1 =
                transactionDbContext1.GetCollection<Foo>();

            IMongoCollection<Foo>  transactionCollection2 =
                transactionDbContext2.GetCollection<Foo>();

            // Act
            transactionCollection1.InsertOne(new Foo { Id = 1, FooName = "Foo1a" });
            await transactionDbContext1.CommitAsync();

            transactionCollection2.InsertOne(new Foo { Id = 544656454, FooName = "Foo1b" });            
            await transactionDbContext2.CommitAsync();
            
            // Assert
            Snapshot.Match(_mongoDatabase.DumpAllCollections());
        }

        #endregion

        #region Private Helpers

        private class TestMongoDbContext : MongoDbContext
        {
            public TestMongoDbContext(MongoOptions mongoOptions) : base(mongoOptions)
            {
            }

            public TestMongoDbContext(MongoOptions mongoOptions, bool enableAutoInit)
                : base(mongoOptions, enableAutoInit)
            {
            }

            protected override void OnConfiguring(IMongoDatabaseBuilder mongoDatabaseBuilder)
            {
                mongoDatabaseBuilder
                    .RegisterCamelCaseConventionPack()
                    .RegisterSerializer<DateTimeOffset>(new DateTimeOffsetSerializer(BsonType.String))
                    .ConfigureConnection(setting => setting.ConnectionMode = ConnectionMode.Automatic)
                    .ConfigureCollection(new FooCollectionConfiguration());
            }

            private class FooCollectionConfiguration : IMongoCollectionConfiguration<Foo>
            {
                public void OnConfiguring(IMongoCollectionBuilder<Foo> mongoCollectionBuilder)
                {
                    mongoCollectionBuilder
                        .AddBsonClassMap<Foo>(cm =>
                        {
                            cm.AutoMap();
                            cm.MapIdMember(c => c.Id);
                        });
                }
            }
        }

        #endregion
    }
}
