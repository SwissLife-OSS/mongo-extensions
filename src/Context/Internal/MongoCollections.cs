using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context.Internal
{
    internal class MongoCollections : IMongoCollections
    {
        private readonly Dictionary<Type, object> _collections;

        public MongoCollections()
        {
            _collections = new Dictionary<Type, object>();
        }

        public int Count => _collections.Count;

        public bool Exists<TDocument>()
            where TDocument : class
        {
            return _collections.ContainsKey(typeof(TDocument));
        }

        public void Add<TDocument>(IMongoCollection<TDocument> mongoCollection)
            where TDocument : class
        {
            _collections.Add(
                typeof(TDocument),
                mongoCollection);
        }

        public IMongoCollection<TDocument>? TryGetCollection<TDocument>()
            where TDocument : class
        {
            if (_collections.TryGetValue(
                typeof(TDocument), out object? configuredMongoCollection))
            {
                return (IMongoCollection<TDocument>)configuredMongoCollection;
            }

            return null;
        }
    }
}
