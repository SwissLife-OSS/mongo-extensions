## [![Nuget](https://img.shields.io/nuget/v/MongoDB.Extensions.Context.svg?style=flat)](https://www.nuget.org/packages/MongoDB.Extensions.Context) [![GitHub Release](https://img.shields.io/github/release/SwissLife-OSS/mongo-extensions.svg?style=flat)](https://github.com/SwissLife-OSS/Mongo-extensions/releases/latest) [![Build Status](https://dev.azure.com/swisslife-oss/swisslife-oss/_apis/build/status/MongoDB.Extensions.Release?branchName=master)](https://dev.azure.com/swisslife-oss/swisslife-oss/_build/latest?definitionId=11&branchName=master) 

**MongoDB.Extensions provides a set of utility libraries for MongoDB.**

MongoDB.Extensions provides several libraries to extend and simplify some MongoDB functionalities like bootstrapping and transactions.

The MongoDB.Extension.Context library provides a bootstrapping context, which is used to initialize the MongoDB connections, databases and collections in a specific and proper way.

## Features

- [x] MongoDB Bootstrapping
- [ ] MongoDB Transactions (InProgress)

## Getting Started - MongoDB Bootstrapping

To get started with MongoDB bootstrapping, we have prepared a complete example at [SimpleBlog](https://swisslife-oss.github.io/mongo-extensions/samples/), which is a small REST web-service with the used MongoDB bootstrapping context.

### Install

Install the MongoDB.Extensions.Context nuget package for MongoDB bootstrapping:

```bash
dotnet add package MongoDB.Extensions.Context
```

### Configure MongoDB Bootstrapping Context

Create a new class and inherit from the MongoDbContext (abstract) class. Add the constructor and override the abstract OnConfiguring method. The MongoOptions only contains the connection string and the database name.

```csharp
public class SimpleBlogDbContext : MongoDbContext
{
    public SimpleBlogDbContext(MongoOptions mongoOptions) : base(mongoOptions)
    {
    }

    protected override void OnConfiguring(IMongoDatabaseBuilder mongoDatabaseBuilder)
    {
        .
        .
        .
    }
}
```

In the OnConfiguring method, the MongoDatabaseBuilder is injected. Use this builder to configure your MongoDB connection, database, collections, convention packs, serializer... etc.

```csharp
protected override void OnConfiguring(IMongoDatabaseBuilder mongoDatabaseBuilder)
{
    mongoDatabaseBuilder
        .RegisterCamelCaseConventionPack()
        .RegisterSerializer(new DateTimeOffsetSerializer())
        .ConfigureConnection(con => con.ReadConcern = ReadConcern.Majority)
        .ConfigureConnection(con => con.WriteConcern = WriteConcern.WMajority)
        .ConfigureConnection(con => con.ReadPreference = ReadPreference.Primary)
        .ConfigureCollection(new TagCollectionConfiguration());
}
```

To configure a collection of your MongoDB database, create a class with the interface ```IMongoCollectionConfiguration<TDocument>``` and
register it in the MongoDatabaseBuilder ```.ConfigureCollection(new TagCollectionConfiguration())``` of your MongoDbContext. Configure the collection settings via the injected MongoCollectionBuilder...

```csharp
public class TagCollectionConfiguration : IMongoCollectionConfiguration<Tag>
{
    public void OnConfiguring(IMongoCollectionBuilder<Tag> mongoCollectionBuilder)
    {
        mongoCollectionBuilder
                .AddBsonClassMap<Tag>(cm => 
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                })
                .WithCollectionSettings(setting =>
                {
                    setting.ReadPreference = ReadPreference.Nearest;
                    setting.ReadConcern = ReadConcern.Available;
                    setting.WriteConcern = WriteConcern.Acknowledged;
                })
                .WithCollectionConfiguration(collection =>
                {
                    var timestampIndex = new CreateIndexModel<Tag>(
                        Builders<Tag>.IndexKeys.Ascending(tag => tag.Name),
                        new CreateIndexOptions { Unique = true });

                    collection.Indexes.CreateOne(timestampIndex);
                });
    }
}
```

### Register MongoDB Context

To use your MongoDB bootstrapping context, register it in your DI-Container.
Example:

```csharp
 public static IServiceCollection AddDatabase(
        this IServiceCollection services, IConfiguration configuration)
        {
            MongoOptions blogDbOptions = configuration
                .GetMongoOptions("SimpleBlog:Database");

            services.AddSingleton(blogDbOptions);
            services.AddSingleton<SimpleBlogDbContext>();

            return services;
        }
```

When the MongoDBContext is used the first time, then the connection, database and collections, serializers, classMaps, convention packs etc. will be initialized and configured according your configuration.

### Use MongoDB Context
The MongoDbContext contains the configured MongoDB client, database and collections. Therefore we should use always the MongoDbContext to get the client, database or a collection, because they are configured correctly.

```csharp
public abstract class MongoDbContext : IMongoDbContext
    {
        .
        .
        .
        public IMongoClient Client { get; }
        public IMongoDatabase Database { get; }
        public MongoOptions MongoOptions { get; }

        public IMongoCollection<TDocument> CreateCollection<TDocument>() where TDocument : class;
        .
        .
        .
    }
```

In the following Repository class example, we use the MongoDbContext to get the configured MongoDB collection.

```csharp
public class TagRepository : ITagRepository
    {
        private IMongoCollection<Tag> _mongoCollection;

        public TagRepository(ISimpleBlogDbContext simpleBlogDbContext)
        {
            if (simpleBlogDbContext == null)
                throw new ArgumentNullException(nameof(simpleBlogDbContext));

            _mongoCollection = simpleBlogDbContext.CreateCollection<Tag>();
        }

        public async Task<IEnumerable<Tag>> GetTagsAsync(
            CancellationToken cancellationToken = default)
        {
            var findOptions = new FindOptions<Tag>();

            IAsyncCursor<Tag> result = await _mongoCollection.FindAsync<Tag>(
                Builders<Tag>.Filter.Empty, findOptions, cancellationToken);

            return await result.ToListAsync();
        }
        .
        .
        .
        .
```

A full MongoDB bootstrapping example can be found in our [SimpleBlog](https://swisslife-oss.github.io/mongo-extensions/samples/) web-application.

## Community

This project has adopted the code of conduct defined by the [Contributor Covenant](https://contributor-covenant.org/)
to clarify expected behavior in our community. For more information, see the [Swiss Life OSS Code of Conduct](https://swisslife-oss.github.io/coc).
