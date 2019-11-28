using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace MongoDB.Bootstrapper
{
    internal class MongoDbContextData
    {
        private readonly Dictionary<Type, object> _mongoCollectionBuilders;

        public MongoDbContextData(
            IMongoClient mongoClient,
            IMongoDatabase mongoDatabase,
            Dictionary<Type, object> mongoCollectionBuilders)
        {
            //TODO check for nulls
            MongoClient = mongoClient;
            MongoDatabase = mongoDatabase;
            _mongoCollectionBuilders = mongoCollectionBuilders;
        }

        public IMongoClient MongoClient { get; }
        public IMongoDatabase MongoDatabase { get; }

        internal IMongoCollection<TDocument> CreateCollection<TDocument>() where TDocument : class
        {
            MongoCollectionBuilder<TDocument> collectionBuilder =
                TryGetCollectionBuilder<TDocument>();

            if (collectionBuilder == null)
            {
                collectionBuilder = new MongoCollectionBuilder<TDocument>(MongoDatabase);
            }

            return collectionBuilder.Build();
        }

        private MongoCollectionBuilder<TDocument> TryGetCollectionBuilder<TDocument>() where TDocument : class
        {
            MongoCollectionBuilder<TDocument> collectionBuilder = null;

            if (_mongoCollectionBuilders.ContainsKey(typeof(TDocument)))
            {
                collectionBuilder = (MongoCollectionBuilder<TDocument>)
                    _mongoCollectionBuilders[typeof(TDocument)];
            }

            return collectionBuilder;
        }
    }
}
