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
            
            Client = mongoClient ?? 
                throw new ArgumentNullException(nameof(mongoClient));
            Database = mongoDatabase ??
                throw new ArgumentNullException(nameof(mongoDatabase));
            _mongoCollectionBuilders = mongoCollectionBuilders ??
                throw new ArgumentNullException(nameof(mongoCollectionBuilders));
        }

        public IMongoClient Client { get; }
        public IMongoDatabase Database { get; }

        internal IMongoCollection<TDocument> CreateCollection<TDocument>() where TDocument : class
        {
            MongoCollectionBuilder<TDocument> collectionBuilder =
                TryGetCollectionBuilder<TDocument>();

            if (collectionBuilder == null)
            {
                collectionBuilder = new MongoCollectionBuilder<TDocument>(Database);
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
