using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context
{
    internal class MongoCollectionBuilder<TDocument> : IMongoCollectionBuilder<TDocument>
    {
        private string _collectionName;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly List<Action> _classMapActions;
        private readonly List<Action<MongoCollectionSettings>> _collectionSettingsActions;
        private readonly List<Action<IMongoCollection<TDocument>>> _collectionConfigurations;

        // static lock for static BsonClassMap
        private static readonly object _lockObject = new object();

        public MongoCollectionBuilder(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
            _collectionName = typeof(TDocument).Name;
            _classMapActions = new List<Action>();
            _collectionConfigurations = new List<Action<IMongoCollection<TDocument>>>();
            _collectionSettingsActions = new List<Action<MongoCollectionSettings>>();
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
            _classMapActions.Add(() => 
                RegisterClassMapSync<TMapDocument>(bsonClassMapAction));

            return this;
        }

        public IMongoCollectionBuilder<TDocument> AddBsonClassMap<TMapDocument>() 
            where TMapDocument : class
        {
            _classMapActions.Add(() => 
                RegisterClassMapSync<TMapDocument>());

            return this;
        }

        public IMongoCollectionBuilder<TDocument> AddBsonClassMap(
            Type type,
            Action<BsonClassMap>? bsonClassMapAction = default)
        {
            _classMapActions.Add(() =>
                RegisterClassMapSync(type, bsonClassMapAction));

            return this;
        }

        public IMongoCollectionBuilder<TDocument> WithCollectionSettings(
            Action<MongoCollectionSettings> collectionSettings)
        {
            _collectionSettingsActions.Add(collectionSettings);

            return this;
        }

        public IMongoCollectionBuilder<TDocument> WithCollectionConfiguration(
            Action<IMongoCollection<TDocument>> collectionConfiguration)
        {
            _collectionConfigurations.Add(collectionConfiguration);

            return this;
        }

        internal IMongoCollection<TDocument> Build()
        {
            _classMapActions.ForEach(action => action());
            
            IMongoCollection<TDocument> mongoCollection = GetMongoCollection();

            _collectionConfigurations.ForEach(configuration => configuration(mongoCollection));

            return mongoCollection;
        }

        private IMongoCollection<TDocument> GetMongoCollection()
        {
            var mongoCollectionSettings = new MongoCollectionSettings();

            _collectionSettingsActions
                .ForEach(configure => configure(mongoCollectionSettings));

            return _mongoDatabase
                .GetCollection<TDocument>(_collectionName, mongoCollectionSettings);
        }

        private void RegisterClassMapSync<TMapDocument>(
            Action<BsonClassMap<TMapDocument>> bsonClassMapAction) where TMapDocument : class
        {
            lock (_lockObject)
            {
                if (!BsonClassMap.IsClassMapRegistered(typeof(TMapDocument)))
                {
                    BsonClassMap.RegisterClassMap(bsonClassMapAction);
                }
            }
        }

        private void RegisterClassMapSync(
            Type type,
            Action<BsonClassMap>? bsonClassMapAction)
        { 
            lock (_lockObject)
            {
                if (!BsonClassMap.IsClassMapRegistered(type))
                {
                    var classMap = new BsonClassMap(type);
                    bsonClassMapAction?.Invoke(classMap);
                    BsonClassMap.RegisterClassMap(classMap);
                }
            }
        }

        private void RegisterClassMapSync<TMapDocument>()
            where TMapDocument : class
        {
            lock (_lockObject)
            {
                if (!BsonClassMap.IsClassMapRegistered(typeof(TMapDocument)))
                {
                    BsonClassMap.RegisterClassMap<TMapDocument>();
                }
            }
        }
    }
}
