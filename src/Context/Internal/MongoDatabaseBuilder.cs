using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context
{
    internal class MongoDatabaseBuilder : IMongoDatabaseBuilder
    {
        private readonly MongoOptions _mongoOptions;
        private readonly List<Action> _registrationConventionActions;
        private readonly List<Action> _registrationSerializerActions;
        private readonly List<Action<MongoClientSettings>> _mongoClientSettingsActions;
        private readonly List<Action<IMongoDatabase>> _databaseConfigurationActions;
        private readonly List<Action<IMongoDatabase, Dictionary<Type, object>>> _builderActions;

        private static readonly object _lockObject = new object();
        private static readonly Dictionary<string, Type> _registeredSerializers;
        private static readonly Dictionary<string, IConventionPack> _registeredConventionPacks;

        static MongoDatabaseBuilder()
        {
            _registeredSerializers = new Dictionary<string, Type>();
            _registeredConventionPacks = new Dictionary<string, IConventionPack>();
        }

        public MongoDatabaseBuilder(MongoOptions mongoOptions)
        {
            _mongoOptions = mongoOptions;
            _registrationConventionActions = new List<Action>();
            _registrationSerializerActions = new List<Action>();
            _mongoClientSettingsActions = new List<Action<MongoClientSettings>>();
            _databaseConfigurationActions = new List<Action<IMongoDatabase>>();
            _builderActions = new List<Action<IMongoDatabase, Dictionary<Type, object>>>();
        }
        
        public IMongoDatabaseBuilder ConfigureConnection(
            Action<MongoClientSettings> mongoClientSettingsAction)
        {
            _mongoClientSettingsActions.Add(mongoClientSettingsAction);

            return this;
        }

        public IMongoDatabaseBuilder ConfigureCollection<TDocument>(
            IMongoCollectionConfiguration<TDocument> configuration) where TDocument : class
        {
            Action<IMongoDatabase, Dictionary<Type, object>> buildAction =
                (mongoDb, mongoCollectionBuilders) =>
                {
                    if (mongoCollectionBuilders.ContainsKey(typeof(TDocument)))
                    {
                        throw new Exception($"The mongo collection configuration for " +
                            $"document type '{typeof(TDocument)}' already exists.");
                    }

                    var collectionBuilder = new MongoCollectionBuilder<TDocument>(mongoDb);

                    configuration.OnConfiguring(collectionBuilder);
                    collectionBuilder.Build();

                    mongoCollectionBuilders.Add(typeof(TDocument), collectionBuilder);
                };

            _builderActions.Add(buildAction);

            return this;
        }

        public IMongoDatabaseBuilder RegisterCamelCaseConventionPack()
        {
            RegisterConventionPack("camelCase", new ConventionPack
                {
                    new EnumRepresentationConvention(BsonType.String),
                    new CamelCaseElementNameConvention()
                }, t => true);

            return this;
        }

        public IMongoDatabaseBuilder RegisterConventionPack(
            string name, IConventionPack conventionPack, Func<Type, bool> filter)
        {
            _registrationConventionActions.Add(
                () => RegisterConventions(name, conventionPack, filter));

            return this;
        }

        public IMongoDatabaseBuilder RegisterSerializer<T>(IBsonSerializer<T> serializer)
        {
            _registrationSerializerActions.Add(
                () => RegisterBsonSerializer(serializer));

            return this;
        }

        public IMongoDatabaseBuilder ConfigureDatabase(Action<IMongoDatabase> configureDatabase)
        {
            _databaseConfigurationActions.Add(configureDatabase);

            return this;
        }

        internal MongoDbContextData Build()
        {
            // synchronize registration
            lock(_lockObject)
            {
                // register all convention packs
                _registrationConventionActions.ForEach(registration => registration());

                // register all serializers
                _registrationSerializerActions.ForEach(registration => registration());
            }

            // create mongo client settings
            var mongoClientSettings = MongoClientSettings
                .FromConnectionString(_mongoOptions.ConnectionString);

            // set default mongo client settings
            mongoClientSettings = SetDefaultClientSettings(
                mongoClientSettings);

            // set specific mongo client settings
            _mongoClientSettingsActions.ForEach(
                settings => settings(mongoClientSettings));
            
            // create mongo client
            var mongoClient = new MongoClient(mongoClientSettings);

            // create mongo database
            IMongoDatabase mongoDatabase = mongoClient
                .GetDatabase(_mongoOptions.DatabaseName);

            // configure mongo database
            _databaseConfigurationActions.ForEach(configure => configure(mongoDatabase));

            // create mongo collection builders
            var mongoCollectionBuilders = new Dictionary<Type, object>();
            _builderActions.ForEach(
                config => config(mongoDatabase, mongoCollectionBuilders));

            // create configured mongo db context
            return new MongoDbContextData(
                mongoClient, mongoDatabase, mongoCollectionBuilders);
        }

        private MongoClientSettings SetDefaultClientSettings(
            MongoClientSettings mongoClientSettings)
        {
            mongoClientSettings.ReadPreference = ReadPreference.Primary;
            mongoClientSettings.ReadConcern = ReadConcern.Majority;
            mongoClientSettings.WriteConcern = WriteConcern.WMajority.With(journal: true);

            return mongoClientSettings;
        }

        private void RegisterBsonSerializer<T>(IBsonSerializer<T> serializer)
        {
            string typeName = typeof(T).ToString();
            if (_registeredSerializers.TryGetValue(typeName, out Type registeredType))
            {
                if (registeredType != serializer.GetType())
                {
                    throw new BsonSerializationException(
                        $"There is already another " +
                        $"serializer registered for type {typeName}. " +
                        $"Registered serializer is {registeredType.Name}. " +
                        $"New serializer is {serializer.GetType().Name}");
                }

                return;
            }

            BsonSerializer.RegisterSerializer(serializer);

            _registeredSerializers.Add(typeof(T).ToString(), serializer.GetType());
        }

        private void RegisterConventions(string name, IConventionPack conventionPack, Func<Type, bool> filter)
        {
            if (_registeredConventionPacks
                                .TryGetValue(name, out IConventionPack registeredConventionPack))
            {
                IEnumerable<string> registeredNames = registeredConventionPack
                    .Conventions.Select(rcp => rcp.Name);
                IEnumerable<string> newNames = conventionPack
                    .Conventions.Select(cp => cp.Name);

                if (registeredNames.Except(newNames).Any() ||
                    newNames.Except(registeredNames).Any())
                {
                    throw new Exception($"The convention pack with name '{name}' " +
                        $"is already registered with different convention packages " +
                        $"({string.Join(",", registeredNames)}). " +
                        $"These convention packages differ from the new ones " +
                        $"({string.Join(", ", newNames)})");
                }

                return;
            }

            _registeredConventionPacks.Add(name, conventionPack);

            ConventionRegistry.Register(name, conventionPack, filter);
        }
    }
}
