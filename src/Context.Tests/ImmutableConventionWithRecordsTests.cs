using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Squadron;
using Xunit;

namespace MongoDB.Extensions.Context.Tests
{
    public class ImmutableConventionWithRecordsTests
    {
        public class SimpleRecordCase : IClassFixture<MongoResource>
        {
            private readonly MongoDbContextData _context;

            public SimpleRecordCase(MongoResource mongoResource)
            {
                _context = CreateContext(mongoResource);
            }

            [Fact]
            public async Task ApplyConvention_SerializeSuccessful()
            {
                // Arrange, Act and Assert
                await InsertAndFind(_context, new A("a"));
            }

            public record A(string Foo);

        }

        public class RecordWithNullableCase : IClassFixture<MongoResource>
        {
            private readonly MongoDbContextData _context;

            public RecordWithNullableCase(MongoResource mongoResource)
            {
                _context = CreateContext(mongoResource);
            }

            [Theory]
            [InlineData(null)]
            [InlineData(1)]
            public async Task ApplyConvention_SerializeSuccessful(int? secondValue)
            {
                // Arrange, Act and Assert
                await InsertAndFind(_context, new A("a", secondValue));
            }

            [Fact]
            public async Task ApplyConvention_WithValueInDb_SerializeSuccessful()
            {
                // Arrange
                IMongoCollection<A> collectionTyped =
                    _context.Database.GetCollection<A>("test");
                IMongoCollection<BsonDocument> collectionUntyped =
                    _context.Database.GetCollection<BsonDocument>("test");

                // Act
                await collectionUntyped.InsertOneAsync(new BsonDocument
                {
                    { "Foo", "a" }, { "Bar", "42" }
                });

                // Assert
                A result = await collectionTyped.FindSync(FilterDefinition<A>.Empty).FirstAsync();
                result.Should().Be(new A("a", 42));
            }

            [Fact]
            public async Task ApplyConvention_WithoutValueInDb_SerializeSuccessful()
            {
                // Arrange
                IMongoCollection<A> collectionTyped =
                    _context.Database.GetCollection<A>("test");
                IMongoCollection<BsonDocument> collectionUntyped =
                    _context.Database.GetCollection<BsonDocument>("test");

                // Act
                await collectionUntyped.InsertOneAsync(new BsonDocument { { "Foo", "a" } });

                // Assert
                A result = await collectionTyped.FindSync(FilterDefinition<A>.Empty).FirstAsync();
                result.Should().Be(new A("a", null));
            }

            public record A(string Foo, int? Bar)
            {
                public string? BarFoo { get; init; }
                public int FooBar { get; init; }
            };

        }

        public class NestedRecordCase : IClassFixture<MongoResource>
        {
            private readonly MongoDbContextData _context;

            public NestedRecordCase(MongoResource mongoResource)
            {
                _context = CreateContext(mongoResource);
            }

            [Fact]
            public async Task ApplyConvention_SerializeSuccessful()
            {
                // Arrange, Act and Assert
                await InsertAndFind(_context, new A(new B("bar"), "a"));
            }

            public record A(B Foo, string BarFoo);

            public record B(string Bar);

        }

        private static async Task InsertAndFind<T>(MongoDbContextData context, T input) where T : class
        {
            // Arrange
            IMongoCollection<T> collection = context.GetCollection<T>();

            // Act
            await collection.InsertOneAsync(input);

            // Assert
            T result = await collection.FindSync(FilterDefinition<T>.Empty).FirstAsync();
            result.Should().Be(input);
        }

        private static MongoDbContextData CreateContext(MongoResource mongoResource)
        {
            var mongoOptions = new MongoOptions
            {
                ConnectionString = mongoResource.ConnectionString,
                DatabaseName = mongoResource.CreateDatabase().DatabaseNamespace.DatabaseName
            };
            var builder = new MongoDatabaseBuilder(mongoOptions);
            builder.RegisterImmutableConventionPack();
            return builder.Build();
        }
    }
}
