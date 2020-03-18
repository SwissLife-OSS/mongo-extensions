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
                throw new ArgumentNullException(nameof(mongoOptions));

            mongoOptions.Validate();

            MongoOptions = mongoOptions;

            if(enableAutoInitialize)
            {
                Initialize(mongoOptions);
            }
        }

        public IMongoClient Client => _mongoDbContextData.Client;
        public IMongoDatabase Database => _mongoDbContextData.Database;
        public MongoOptions MongoOptions { get; }
        
        public IMongoCollection<TDocument> CreateCollection<TDocument>() where TDocument : class
        {
            return _mongoDbContextData.CreateCollection<TDocument>();
        }
        
        protected abstract void OnConfiguring(IMongoDatabaseBuilder mongoDatabaseBuilder);

        protected void Initialize(MongoOptions mongoOptions)
        {
            if(_mongoDbContextData == null)
            {
                lock (_lockObject)
                {
                    if (_mongoDbContextData == null)
                    {
                        var mongoDatabaseBuilder = new MongoDatabaseBuilder(mongoOptions);

                        OnConfiguring(mongoDatabaseBuilder);

                        _mongoDbContextData = mongoDatabaseBuilder.Build();
                    }
                }
            }
        }
    }
}
