using System;
using MongoDB.Driver;


namespace MongoDB.Extensions.Context
{
    public abstract class MongoDbContext : IMongoDbContext
    {
        private readonly MongoDbContextData _mongoDbContextData;

        public MongoDbContext(MongoOptions mongoOptions)
        {
            if(mongoOptions == null)
                throw new ArgumentNullException(nameof(mongoOptions));

            mongoOptions.Validate();

            MongoOptions = mongoOptions;

            _mongoDbContextData = Initialize(mongoOptions);
        }

        public IMongoClient Client => _mongoDbContextData.Client;
        public IMongoDatabase Database => _mongoDbContextData.Database;
        public MongoOptions MongoOptions { get; }
        
        public IMongoCollection<TDocument> CreateCollection<TDocument>() where TDocument : class
        {
            return _mongoDbContextData.CreateCollection<TDocument>();
        }
        
        protected abstract void OnConfiguring(IMongoDatabaseBuilder mongoDatabaseBuilder);

        private MongoDbContextData Initialize(MongoOptions mongoOptions)
        {
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(mongoOptions);

            OnConfiguring(mongoDatabaseBuilder);

            return mongoDatabaseBuilder.Build();
        }
    }
}
