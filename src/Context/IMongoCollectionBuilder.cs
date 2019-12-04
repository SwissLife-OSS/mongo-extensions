using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context
{
    public interface IMongoCollectionBuilder<TDocument>
    {
        IMongoCollectionBuilder<TDocument> WithCollectionName(string collectionName);

        IMongoCollectionBuilder<TDocument> AddBsonClassMap<TMapDocument>(
            Action<BsonClassMap<TMapDocument>> bsonClassMapAction) where TMapDocument : class;

        IMongoCollectionBuilder<TDocument> WithCollectionSettings(
            Action<MongoCollectionSettings> collectionSettings);

        IMongoCollectionBuilder<TDocument> WithCollectionConfiguration(
            Action<IMongoCollection<TDocument>> collectionConfiguration);        
    }
}
