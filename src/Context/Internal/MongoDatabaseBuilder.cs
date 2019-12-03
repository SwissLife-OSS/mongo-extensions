using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace MongoDB.Bootstrapper
{
    internal class MongoDatabaseBuilder : IMongoDatabaseBuilder
    {
        private readonly MongoOptions _mongoOptions;
        private readonly List<Action> _registrationConventionActions;
        private readonly List<Action> _registrationSerializerActions;
        private readonly List<Action<MongoClientSettings>> _mongoClientSettingsActions;
        private readonly List<Action<IMongoDatabase, Dictionary<Type, object>>> _builderActions;

        private static readonly Dictionary<string, IConventionPack> _registeredConventionPacks;
        
        static MongoDatabaseBuilder()
        {
            _registeredConventionPacks = new Dictionary<string, IConventionPack>();
        }

        public MongoDatabaseBuilder(MongoOptions mongoOptions)
        {
            _mongoOptions = mongoOptions;
            _registrationConventionActions = new List<Action>();
            _registrationSerializerActions = new List<Action>();
            _mongoClientSettingsActions = new List<Action<MongoClientSettings>>();
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
            Action<IMongoDatabase, Dictionary<Type, object>> buildAction = (mongoDb, mongoCollectionBuilders) =>
            {
                if (mongoCollectionBuilders.ContainsKey(typeof(TDocument)))
                {
                    throw new Exception($"The mongo collection configuration for " +
                        $"document type '{typeof(TDocument)}' already exists.");
                }

                var collectionBuilder = new MongoCollectionBuilder<TDocument>(mongoDb);

                configuration.Configure(collectionBuilder);

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
            Action registerConvenctionPackAction = () =>
            {
                if (_registeredConventionPacks
                    .TryGetValue(name, out IConventionPack registeredConventionPack))
                {
                    IEnumerable<string> registeredNames = registeredConventionPack
                        .Conventions.Select(rcp => rcp.Name);
                    IEnumerable<string> newNames = conventionPack
                        .Conventions.Select(cp => cp.Name);

                    // different convention pack names are not allowed for same registration name
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
            };

            _registrationConventionActions.Add(registerConvenctionPackAction);

            return this;
        }

        public IMongoDatabaseBuilder RegisterSerializer<T>(IBsonSerializer<T> serializer)
        {
            Action initAction = () =>
            {
                // TODO Create dictionary with the type and serializer type and if the type does already exist, then check if the serializer type is the same, if not throw exception.
                BsonSerializer.RegisterSerializer<T>(serializer);
            };

            _registrationSerializerActions.Add(initAction);

            return this;
        }
        
        internal MongoDbContextData Build()
        {            
            // register all convention packs
            _registrationConventionActions.ForEach(registration => registration());

            // register all serializers
            _registrationSerializerActions.ForEach(registration => registration());

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
    }
}
