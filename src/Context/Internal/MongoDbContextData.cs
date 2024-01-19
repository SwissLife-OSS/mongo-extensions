using MongoDB.Driver;
using MongoDB.Extensions.Context.Internal;

namespace MongoDB.Extensions.Context
{
    internal class MongoDbContextData
    {
        private readonly IMongoCollections _mongoCollections;
        private readonly object _lockObject = new object();

        public MongoDbContextData(
            IMongoClient mongoClient,
            IMongoDatabase mongoDatabase,
            IMongoCollections mongoCollections)
        {
            Client = mongoClient;
            Database = mongoDatabase;
            _mongoCollections = mongoCollections;
        }

        public IMongoClient Client { get; }
        public IMongoDatabase Database { get; }

        public IMongoCollection<TDocument> GetCollection<TDocument>()
            where TDocument : class
        {
            return GetConfiguredCollection<TDocument>();
        }

        private IMongoCollection<TDocument> GetConfiguredCollection<TDocument>()
            where TDocument : class
        {
            IMongoCollection<TDocument>? configuredCollection =
                _mongoCollections.TryGetCollection<TDocument>();

            if (configuredCollection == null)
            {
                lock (_lockObject)
                {
                    configuredCollection =
                        _mongoCollections.TryGetCollection<TDocument>();

                    if (configuredCollection == null)
                    {
                        configuredCollection =
                            AddDefaultCollection<TDocument>();
                    }
                }
            }

            return configuredCollection;
        }

        private IMongoCollection<TDocument> AddDefaultCollection<TDocument>()
            where TDocument : class
        {
            IMongoCollection<TDocument> configuredCollection =
                new MongoCollectionBuilder<TDocument>(Database).Build();

            _mongoCollections.Add(configuredCollection);

            return configuredCollection;
        }
    }
}
