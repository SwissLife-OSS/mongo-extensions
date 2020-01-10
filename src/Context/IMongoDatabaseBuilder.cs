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

        IMongoDatabaseBuilder RegisterDefaultConventionPack(Func<Type, bool> filter);

        IMongoDatabaseBuilder RegisterCamelCaseConventionPack();

        IMongoDatabaseBuilder ConfigureCollection<TDocument>(
            IMongoCollectionConfiguration<TDocument> configuration) where TDocument : class;

        IMongoDatabaseBuilder ConfigureConnection(
            Action<MongoClientSettings> mongoClientSettingsAction);
    }
}
