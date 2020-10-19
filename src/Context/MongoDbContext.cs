using System;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context
{
    public abstract class MongoDbContext : IMongoDbContext
    {
        private MongoDbContextData _mongoDbContextData;

        private readonly object _lockObject = new object();

        public MongoDbContext(MongoOptions mongoOptions) : this(mongoOptions, false)
        {
        }

        [Obsolete]
        public MongoDbContext(MongoOptions mongoOptions, bool enableAutoInitialize)
        {
            if (mongoOptions == null)
                throw new ArgumentNullException(nameof(mongoOptions));

            mongoOptions.Validate();

            MongoOptions = mongoOptions;

            // This initialization should be removed and switched to Lazy initialization.
            if (enableAutoInitialize)
            {
                Initialize(mongoOptions);
            }
        }

        public IMongoClient Client
        {
            get
            {
                EnsureInitialized();
                return _mongoDbContextData.Client;
            }
        }

        public IMongoDatabase Database
        {
            get
            {
                EnsureInitialized();
                return _mongoDbContextData.Database;
            }
        }

        public MongoOptions MongoOptions { get; }
        
        public IMongoCollection<TDocument> CreateCollection<TDocument>()
            where TDocument : class
        {
            EnsureInitialized();
            return _mongoDbContextData.CreateCollection<TDocument>();
        }
        
        protected abstract void OnConfiguring(IMongoDatabaseBuilder mongoDatabaseBuilder);

        private void EnsureInitialized()
        {
            Initialize(MongoOptions);
        }

        [Obsolete]
        protected void Initialize(MongoOptions mongoOptions)
        {
            if(_mongoDbContextData == null)
            {
                lock (_lockObject)
                {
                    if (_mongoDbContextData == null)
                    {
                        var mongoDatabaseBuilder = new MongoDatabaseBuilder(MongoOptions);

                        OnConfiguring(mongoDatabaseBuilder);

                        _mongoDbContextData = mongoDatabaseBuilder.Build();
                    }
                }
            }
        }
    }
}
