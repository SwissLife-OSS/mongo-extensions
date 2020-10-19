using System;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context
{
    public abstract class MongoDbContext : IMongoDbContext
    {
        private MongoDbContextData _mongoDbContextData;

        private readonly object _lockObject = new object();

        protected MongoDbContext(MongoOptions mongoOptions)
        {
            if (mongoOptions == null)
            {
                throw new ArgumentNullException(nameof(mongoOptions));
            }

            MongoOptions = mongoOptions.Validate();
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
