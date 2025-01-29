using System;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context
{
    public interface IMongoCollectionBuilder<TDocument>
    {
        IMongoCollectionBuilder<TDocument> WithCollectionName(string collectionName);

        IMongoCollectionBuilder<TDocument> AddBsonClassMap<TMapDocument>()
            where TMapDocument : class;

        IMongoCollectionBuilder<TDocument> AddBsonClassMap<TMapDocument>(
            Action<BsonClassMap<TMapDocument>> bsonClassMapAction)
            where TMapDocument : class;

        IMongoCollectionBuilder<TDocument> AddBsonClassMap(
            Type type,
            Action<BsonClassMap>? bsonClassMapAction = default);

        IMongoCollectionBuilder<TDocument> WithCollectionSettings(
            Action<MongoCollectionSettings> collectionSettings);

        IMongoCollectionBuilder<TDocument> WithCollectionConfiguration(
            Action<IMongoCollection<TDocument>> collectionConfiguration);
    }
}
