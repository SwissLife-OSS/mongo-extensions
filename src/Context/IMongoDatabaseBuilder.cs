using System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace MongoDB.Bootstrapper
{
    public interface IMongoDatabaseBuilder
    {
        IMongoDatabaseBuilder Initialize(Action initialization);

        IMongoDatabaseBuilder RegisterSerializer<T>(IBsonSerializer<T> serializer);

        IMongoDatabaseBuilder RegisterConventionPack(
            string name, IConventionPack conventions, Func<Type, bool> filter);

        IMongoDatabaseBuilder RegisterCamelCaseConventionPack();

        IMongoDatabaseBuilder ConfigureCollection<TDocument>(
            IMongoCollectionConfiguration<TDocument> configuration) where TDocument : class;

        IMongoDatabaseBuilder ConfigureConnection(
            Action<MongoClientSettings> mongoClientSettingsAction);
    }
}
