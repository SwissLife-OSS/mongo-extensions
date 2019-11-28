using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MongoDB.Bootstrapper
{
    internal class MongoCollectionBuilder<TDocument> : IMongoCollectionBuilder<TDocument>
    {
        private string _collectionName;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly List<Action<MongoCollectionSettings>> _collectionSettingsActions;
        private readonly List<Action<IMongoCollection<TDocument>>> _collectionConfigurations;

        public MongoCollectionBuilder(IMongoDatabase mongoDatabase)
        {
            _collectionName = typeof(TDocument).Name;
            _collectionConfigurations = new List<Action<IMongoCollection<TDocument>>>();
            _collectionSettingsActions = new List<Action<MongoCollectionSettings>>();

            _mongoDatabase = mongoDatabase ??
                throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public IMongoCollectionBuilder<TDocument> WithCollectionName(string collectionName)
        {
            if (string.IsNullOrEmpty(collectionName))
                throw new ArgumentException("Mongo collection name must not be null or empty.");

            _collectionName = collectionName;

            return this;
        }

        public IMongoCollectionBuilder<TDocument> AddBsonClassMap<TMapDocument>(
            Action<BsonClassMap<TMapDocument>> bsonClassMapAction) where TMapDocument : class
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(TMapDocument)))
            {
                BsonClassMap.RegisterClassMap(bsonClassMapAction);
            }

            return this;
        }

        public IMongoCollectionBuilder<TDocument> WithMongoCollectionSettings(
            Action<MongoCollectionSettings> collectionSettings)
        {
            _collectionSettingsActions.Add(collectionSettings);

            return this;
        }

        public IMongoCollectionBuilder<TDocument> WithMongoCollectionConfiguration(
            Action<IMongoCollection<TDocument>> collectionConfiguration)
        {
            _collectionConfigurations.Add(collectionConfiguration);

            return this;
        }

        internal IMongoCollection<TDocument> Build()
        {
            MongoCollectionSettings mongoCollectionSettings = null;

            if (_collectionSettingsActions.Count != 0)
            {
                mongoCollectionSettings = new MongoCollectionSettings();
                _collectionSettingsActions
                    .ForEach(configure => configure(mongoCollectionSettings));
            }

            IMongoCollection<TDocument> mongoCollection = _mongoDatabase
                .GetCollection<TDocument>(_collectionName, mongoCollectionSettings);

            _collectionConfigurations
                .ForEach(configuration => configuration(mongoCollection));

            return mongoCollection;
        }
    }
}
