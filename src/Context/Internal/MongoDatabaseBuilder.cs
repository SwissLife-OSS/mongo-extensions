using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Extensions.Context.Internal;

namespace MongoDB.Extensions.Context;

internal class MongoDatabaseBuilder : IMongoDatabaseBuilder
{
    private readonly MongoOptions _mongoOptions;
    private readonly List<Action> _registrationConventionActions;
    private readonly List<Action> _registrationSerializerActions;
    private readonly List<Action<MongoClientSettings>> _mongoClientSettingsActions;
    private readonly List<Action<IMongoDatabase>> _databaseConfigurationActions;
    private readonly List<Action<IMongoDatabase, IMongoCollections>> _collectionActions;

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
        _collectionActions = new List<Action<IMongoDatabase, IMongoCollections>>();
    }

    public IMongoDatabaseBuilder ConfigureConnection(
        Action<MongoClientSettings> mongoClientSettingsAction)
    {
        _mongoClientSettingsActions.Add(mongoClientSettingsAction);
        return this;
    }

    public IMongoDatabaseBuilder ConfigureClientSettings(
        Action<MongoClientSettings> mongoClientSettingsAction)
    {
        _mongoClientSettingsActions.Add(mongoClientSettingsAction);
        return this;
    }

    public IMongoDatabaseBuilder AddAllowedType<T>()
    {
        TypeObjectSerializer.AddAllowedType<T>();
        return this;
    }

    public IMongoDatabaseBuilder AddAllowedTypes(params Type[] allowedTypes)
    {
        TypeObjectSerializer.AddAllowedTypes(allowedTypes);
        return this;
    }

    public IMongoDatabaseBuilder AddAllowedTypes(params string[] allowedNamespaces)
    {
        TypeObjectSerializer.AddAllowedTypes(allowedNamespaces);
        return this;
    }

    public IMongoDatabaseBuilder AddAllowedTypesOfAllDependencies(params string[] excludeNamespaces)
    {
        TypeObjectSerializer.AddAllowedTypesOfAllDependencies(excludeNamespaces);
        return this;
    }

    public IMongoDatabaseBuilder ClearAllowedTypes()
    {
        TypeObjectSerializer.Clear();
        return this;
    }

    public IMongoDatabaseBuilder ConfigureCollection<TDocument>(
        IMongoCollectionConfiguration<TDocument> configuration) where TDocument : class
    {
        Action<IMongoDatabase, IMongoCollections> collectionConfigurationAction =
            (mongoDb, mongoCollectionBuilders) =>
            {
                if (mongoCollectionBuilders.Exists<TDocument>())
                {
                    throw new Exception($"The mongo collection configuration for " +
                        $"document type '{typeof(TDocument)}' already exists.");
                }

                var collectionBuilder = new MongoCollectionBuilder<TDocument>(mongoDb);

                configuration.OnConfiguring(collectionBuilder);

                IMongoCollection<TDocument> configuredCollection =
                    collectionBuilder.Build();

                mongoCollectionBuilders.Add(configuredCollection);
            };

        _collectionActions.Add(collectionConfigurationAction);

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

    public IMongoDatabaseBuilder RegisterDefaultConventionPack()
    {
        var conventionPack = new ConventionPack
        {
            new EnumRepresentationConvention(BsonType.String),
            new ImmutableConvention(),
            new IgnoreExtraElementsConvention(true)
        };
        RegisterConventionPack("Default", conventionPack, t => true);

        return this;
    }

    public IMongoDatabaseBuilder RegisterImmutableConventionPack()
    {
        RegisterConventionPack("Immutable", new ConventionPack
        {
            new ImmutableConvention(),
            new IgnoreExtraElementsConvention(true)
        }, t => true);

        return this;
    }

    public IMongoDatabaseBuilder RegisterIgnoreIfNullConventionPack()
    {
        RegisterConventionPack("IgnoreIfNull", new ConventionPack
        {
            new IgnoreIfNullConvention(true)
        }, t => true);

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

            // add object serializer if not exists
            TryRegisterObjectSerializer();
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
        _databaseConfigurationActions.ForEach(
            configure => configure(mongoDatabase));

        // create mongo collections
        var mongoCollections = new MongoCollections();

        // configure mongo collections
        _collectionActions.ForEach(
            config => config(mongoDatabase, mongoCollections));

        // create configured mongo db context data
        return new MongoDbContextData(
            mongoClient, mongoDatabase, mongoCollections);
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
        if (_registeredConventionPacks.TryGetValue(name, out IConventionPack registeredConventionPack))
        {
            IEnumerable<string> registeredNames = registeredConventionPack
                .Conventions.Select(rcp => rcp.Name).ToImmutableList();
            IEnumerable<string> newNames = conventionPack
                .Conventions.Select(cp => cp.Name).ToImmutableList();

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

    private void TryRegisterObjectSerializer()
    {
        if (!_registeredSerializers.ContainsKey(typeof(object).ToString()))
        {
            var typeObjectSerializer = new TypeObjectSerializer();

            RegisterBsonSerializer<object>(typeObjectSerializer.ObjectSerializer);
        }
    }
}
