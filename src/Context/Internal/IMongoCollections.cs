using MongoDB.Driver;

namespace MongoDB.Extensions.Context.Internal
{
    internal interface IMongoCollections
    {
        bool Exists<TDocument>()
            where TDocument : class;

        void Add<TDocument>(IMongoCollection<TDocument> mongoCollection)
            where TDocument : class;

        IMongoCollection<TDocument>? TryGetCollection<TDocument>()
            where TDocument : class;
    }
}
