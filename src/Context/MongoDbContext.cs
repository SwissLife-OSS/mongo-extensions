using System;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context
{
    public abstract class MongoDbContext : IMongoDbContext
    {
        private MongoDbContextData _mongoDbContextData;

        private readonly object _lockObject = new object();

        public MongoDbContext(MongoOptions mongoOptions) : this(mongoOptions, true)
        {
        }

        public MongoDbContext(MongoOptions mongoOptions, bool enableAutoInitialize)
        {
            if (mongoOptions == null)
            {
                throw new ArgumentNullException(nameof(mongoOptions));
            }

            MongoOptions = mongoOptions.Validate();

            if (enableAutoInitialize)
            {
                Initialize();
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
            if (_mongoDbContextData == null)
            {
                lock (_lockObject)
                {
                    if (_mongoDbContextData == null)
                    {
                        throw new InvalidOperationException("MongoDbContext not initialized.");
                    }
                }
            }
        }

        public virtual void Initialize()
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
