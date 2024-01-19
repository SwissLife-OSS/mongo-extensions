using System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context
{
    public interface IMongoDatabaseBuilder
    {
        IMongoDatabaseBuilder RegisterSerializer<T>(IBsonSerializer<T> serializer);

        IMongoDatabaseBuilder RegisterConventionPack(
            string name, IConventionPack conventions, Func<Type, bool> filter);

        IMongoDatabaseBuilder RegisterImmutableConventionPack();

        IMongoDatabaseBuilder RegisterDefaultConventionPack();

        IMongoDatabaseBuilder RegisterCamelCaseConventionPack();

        IMongoDatabaseBuilder RegisterIgnoreIfNullConventionPack();

        IMongoDatabaseBuilder ConfigureCollection<TDocument>(
            IMongoCollectionConfiguration<TDocument> configuration) where TDocument : class;

        IMongoDatabaseBuilder ConfigureConnection(
            Action<MongoClientSettings> mongoClientSettingsAction);

        IMongoDatabaseBuilder ConfigureDatabase(Action<IMongoDatabase> configureDatabase);

        IMongoDatabaseBuilder AddAllowedType<T>();

        IMongoDatabaseBuilder AddAllowedTypes(params Type[] allowedTypes);

        IMongoDatabaseBuilder AddAllowedTypes(params string[] allowedNamespaces);

        IMongoDatabaseBuilder AddAllowedTypesOfAllDependencies();
    }
}
