using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Snapshooter.Xunit;
using Squadron;
using Xunit;

namespace MongoDB.Extensions.Context.Tests
{
    public class ImmutableConventionTests
    {
        public class SimpleImmutableCase : IClassFixture<MongoResource>
        {
            private readonly MongoDbContextData _context;

            public SimpleImmutableCase(MongoResource mongoResource)
            {
                _context = CreateContext(mongoResource);
            }

            [Fact]
            public async Task ApplyConvention_SerializeSuccessful()
            {
                // Arrange
                IMongoCollection<A> collection = _context.CreateCollection<A>();

                // Act
                await collection.InsertOneAsync(new A("a"));

                // Assert
                A result = await collection.FindSync(FilterDefinition<A>.Empty).FirstAsync();
                result.MatchSnapshot();
            }

            public class A
            {
                public A(string _a)
                {
                    _A = _a;
                }

                public string _A { get; }
            }
        }

        public class SimpleImmutableWithInterfaceCase : IClassFixture<MongoResource>
        {
            private readonly MongoDbContextData _context;

            public SimpleImmutableWithInterfaceCase(MongoResource mongoResource)
            {
                _context = CreateContext(mongoResource);
            }

            [Fact]
            public async Task ApplyConvention_SerializeSuccessful()
            {
                // Arrange
                IMongoCollection<IA> collection = _context.CreateCollection<IA>();

                // Act
                await collection.InsertOneAsync(new A("a"));

                // Assert
                A result = await collection.FindSync(FilterDefinition<IA>.Empty).FirstAsync() as A;
                result.MatchSnapshot();
            }

            public class A : IA
            {
                public A(string _a)
                {
                    _A = _a;
                }

                public string _A { get; }
            }

            public interface IA
            {
                string _A { get; }
            }
        }

        public class NullableReferenceTypeCase : IClassFixture<MongoResource>
        {
            private readonly MongoDbContextData _context;

            public NullableReferenceTypeCase(MongoResource mongoResource)
            {
                _context = CreateContext(mongoResource);
            }

            [Fact]
            public async Task ApplyConvention_WithoutValue_SerializeSuccessful()
            {
                // Arrange
                IMongoCollection<A> collection = _context.CreateCollection<A>();

                // Act
                await collection.InsertOneAsync(new A("a"));

                // Assert
                A result = await collection.FindSync(FilterDefinition<A>.Empty).FirstAsync();
                result.MatchSnapshot();
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
                await collectionUntyped.InsertOneAsync(new BsonDocument {{"_A", "a"}});

                // Assert
                A result = await collectionTyped.FindSync(FilterDefinition<A>.Empty).FirstAsync();
                result.MatchSnapshot();
            }

            [Fact]
            public async Task ApplyConvention_WithValue_SerializeSuccessful()
            {
                // Arrange
                IMongoCollection<A> collection = _context.CreateCollection<A>();

                // Act
                await collection.InsertOneAsync(new A("a", "b"));

                // Assert
                A result = await collection.FindSync(FilterDefinition<A>.Empty).FirstAsync();
                result.MatchSnapshot();
            }

            public class A
            {
                public A(string _a, string? _b = default)
                {
                    _A = _a;
                    _B = _b;
                }

                public string _A { get; }
                public string? _B { get; }
            }
        }

        public class NullableValueTypeCase : IClassFixture<MongoResource>
        {
            private readonly MongoDbContextData _context;

            public NullableValueTypeCase(MongoResource mongoResource)
            {
                _context = CreateContext(mongoResource);
            }

            [Fact]
            public async Task ApplyConvention_WithoutValue_SerializeSuccessful()
            {
                // Arrange
                IMongoCollection<A> collection = _context.CreateCollection<A>();

                // Act
                await collection.InsertOneAsync(new A("a"));

                // Assert
                A result = await collection.FindSync(FilterDefinition<A>.Empty).FirstAsync();
                result.MatchSnapshot();
            }

            [Fact]
            public async Task ApplyConvention_WithValue_SerializeSuccessful()
            {
                // Arrange
                IMongoCollection<A> collection = _context.CreateCollection<A>();

                // Act
                await collection.InsertOneAsync(new A("a", 9));

                // Assert
                A result = await collection.FindSync(FilterDefinition<A>.Empty).FirstAsync();
                result.MatchSnapshot();
            }

            public class A
            {
                public A(string _a, int? _b = default)
                {
                    _A = _a;
                    _B = _b;
                }

                public string _A { get; }
                public int? _B { get; }
            }
        }

        public class AbstractImmutableWithBasePropertyCase : IClassFixture<MongoResource>
        {
            private readonly MongoDbContextData _context;

            public AbstractImmutableWithBasePropertyCase(MongoResource mongoResource)
            {
                _context = CreateContext(mongoResource);
            }

            [Fact]
            public async Task ApplyConvention_SerializeSuccessful()
            {
                // Arrange
                IMongoCollection<A> collection = _context.CreateCollection<A>();

                // Act
                await collection.InsertOneAsync(new B("a", "b"));

                // Assert
                B result = await collection.FindSync(FilterDefinition<A>.Empty).FirstAsync() as B;
                result.MatchSnapshot();
            }

            public abstract class A
            {
                protected A(string _a)
                {
                    _A = _a;
                }

                public string _A { get; }
            }

            public class B : A
            {
                public B(string _a, string _b)
                    : base(_a)
                {
                    _B = _b;
                }

                public string _B { get; }
            }
        }

        public class AbstractImmutableWithAbstractBasePropertyCase : IClassFixture<MongoResource>
        {
            private readonly MongoDbContextData _context;

            public AbstractImmutableWithAbstractBasePropertyCase(MongoResource mongoResource)
            {
                _context = CreateContext(mongoResource);
            }

            [Fact]
            public async Task ApplyConvention_SerializeSuccessful()
            {
                // Arrange
                IMongoCollection<A> collection = _context.CreateCollection<A>();

                // Act
                await collection.InsertOneAsync(new B("a", "b"));

                // Assert
                B result = await collection.FindSync(FilterDefinition<A>.Empty).FirstAsync() as B;
                result.MatchSnapshot();
            }

            public abstract class A
            {
                public abstract string _A { get; }
            }

            public class B : A
            {
                public B(string _a, string _b)
                {
                    _A = _a;
                    _B = _b;
                }

                public override string _A { get; }
                public string _B { get; }
            }
        }

        public class AbstractImmutableWithVirtualBasePropertyCase : IClassFixture<MongoResource>
        {
            private readonly MongoDbContextData _context;

            public AbstractImmutableWithVirtualBasePropertyCase(MongoResource mongoResource)
            {
                _context = CreateContext(mongoResource);
            }

            [Fact]
            public async Task ApplyConvention_SerializeSuccessful()
            {
                // Arrange
                IMongoCollection<A> collection = _context.CreateCollection<A>();

                // Act
                await collection.InsertOneAsync(new B("a", "b"));

                // Assert
                B result = await collection.FindSync(FilterDefinition<A>.Empty).FirstAsync() as B;
                result.MatchSnapshot();
            }

            public abstract class A
            {
                protected A(string _a)
                {
                    _A = _a;
                }

                public virtual string _A { get; }
            }

            public class B : A
            {
                public B(string _a, string _b)
                    : base(_a)
                {
                    _B = _b;
                }

                public string _B { get; }
            }
        }

        public class AbstractImmutableWithNullableVirtualBasePropertyCase : IClassFixture<MongoResource>
        {
            private readonly MongoDbContextData _context;

            public AbstractImmutableWithNullableVirtualBasePropertyCase(MongoResource mongoResource)
            {
                _context = CreateContext(mongoResource);
            }

            [Fact]
            public async Task ApplyConvention_SerializeSuccessful()
            {
                // Arrange
                IMongoCollection<A> collection = _context.CreateCollection<A>();

                // Act
                await collection.InsertOneAsync(new B("b"));

                // Assert
                B result = await collection.FindSync(FilterDefinition<A>.Empty).FirstAsync() as B;
                result.MatchSnapshot();
            }

            public abstract class A
            {
                public virtual string? _A { get; }
            }

            public class B : A
            {
                public B(string _b)
                {
                    _B = _b;
                }

                public string _B { get; }
            }
        }

        public class AbstractImmutableWithVirtualBasePropertyOverriddenCase : IClassFixture<MongoResource>
        {
            private readonly MongoDbContextData _context;

            public AbstractImmutableWithVirtualBasePropertyOverriddenCase(MongoResource mongoResource)
            {
                _context = CreateContext(mongoResource);
            }

            [Fact(Skip = "Case to be covered")]
            public async Task ApplyConvention_SerializeSuccessful()
            {
                // Arrange
                IMongoCollection<A> collection = _context.CreateCollection<A>();

                // Act
                await collection.InsertOneAsync(new B("a", "b"));

                // Assert
                B result = await collection.FindSync(FilterDefinition<A>.Empty).FirstAsync() as B;
                result.MatchSnapshot();
            }

            public abstract class A
            {
                public virtual string? _A { get; }
            }

            public class B : A
            {
                public B(string _a, string _b)
                {
                    _A = _a;
                    _B = _b;
                }

                public override string _A { get; }
                public string _B { get; }
            }
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
