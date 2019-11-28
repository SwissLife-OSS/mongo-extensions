using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace MongoDB.Bootstrapper
{
    internal class MongoDatabaseBuilder : IMongoDatabaseBuilder
    {
        private readonly MongoOptions _mongoOptions;
        private readonly List<Action> _registrationActions;
        private readonly List<Action<MongoClientSettings>> _mongoClientSettingsActions;
        private readonly List<Action<IMongoDatabase, Dictionary<Type, object>>> _builderActions;
        
        public MongoDatabaseBuilder(MongoOptions mongoOptions)
        {
            _mongoOptions = mongoOptions;
            _registrationActions = new List<Action>();
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
            string name, IConventionPack conventions, Func<Type, bool> filter)
        {
            Action initAction = () =>
            {
                // TODO Create a dictionary with the name and type and if the name already exists, then check if the save conventions of the conventionpacks are registered, if not than throw an exception.
                // Register when name not exist
                // Register when name exists and the conventions types are the same
                // Throw exception if the name exist, but conventions are different.
                //ConventionRegistry.Remove(name);
                IConventionPack dd = ConventionRegistry.Lookup(typeof(string));

                IEnumerable<IConvention> daa = dd.Conventions;

                ConventionRegistry.Register(name, conventions, filter);

                IConventionPack ddaaa = ConventionRegistry.Lookup(typeof(string));
                IEnumerable<IConvention> daaadfa = ddaaa.Conventions;
            };

            _registrationActions.Add(initAction);

            return this;
        }

        public IMongoDatabaseBuilder RegisterSerializer<T>(IBsonSerializer<T> serializer)
        {
            Action initAction = () =>
            {
                // TODO Create dictionary with the type and serializer type and if the type does already exist, then check if the serializer type is the same, if not throw exception.
                BsonSerializer.RegisterSerializer<T>(serializer);                
            };

            _registrationActions.Add(initAction);

            return this;
        }
        
        internal MongoDbContextData Build()
        {            
            _registrationActions.ForEach(init => init());

            var mongoClientSettings = MongoClientSettings
                .FromConnectionString(_mongoOptions.ConnectionString);

            _mongoClientSettingsActions.ForEach(
                settings => settings(mongoClientSettings));
            
            var mongoClient = new MongoClient(mongoClientSettings);
            IMongoDatabase mongoDatabase = mongoClient
                .GetDatabase(_mongoOptions.DatabaseName);

            var mongoCollectionBuilders = new Dictionary<Type, object>();

            _builderActions.ForEach(
                config => config(mongoDatabase, mongoCollectionBuilders));

            return new MongoDbContextData(
                mongoClient, mongoDatabase, mongoCollectionBuilders);
        }        
    }
}
