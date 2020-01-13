using System.Threading.Tasks;
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
            private MongoDbContextData _context;

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
            private MongoDbContextData _context;

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

        public class AbstractImmutableWithBasePropertyCase : IClassFixture<MongoResource>
        {
            private MongoDbContextData _context;

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
            private MongoDbContextData _context;

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

        private static MongoDbContextData CreateContext(MongoResource mongoResource)
        {
            var mongoOptions = new MongoOptions()
            {
                ConnectionString = mongoResource.ConnectionString,
                DatabaseName = mongoResource.CreateDatabase().DatabaseNamespace.DatabaseName
            };
            var builder = new MongoDatabaseBuilder(mongoOptions);
            builder.RegisterDefaultConventionPack(f => true);
            return builder.Build();
        }
    }
}
