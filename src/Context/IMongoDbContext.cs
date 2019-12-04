using MongoDB.Driver;

namespace MongoDB.Bootstrapper
{
    public interface IMongoDbContext
    {
        MongoOptions MongoOptions { get; }
        IMongoClient Client { get; }
        IMongoDatabase Database { get; }

        IMongoCollection<TDocument> CreateCollection<TDocument>() where TDocument : class;
    }
}
