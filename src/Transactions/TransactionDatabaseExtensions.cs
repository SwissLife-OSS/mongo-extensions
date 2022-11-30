using MongoDB.Driver;

namespace MongoDB.Extensions.Transactions;

public static class TransactionDatabaseExtensions
{
    public static IMongoDatabase AsTransactionDatabase(
        this IMongoDatabase collection)
    {
        return new MongoTransactionDatabase(collection);
    }

    public static IMongoDatabase AsTransactionDatabase(
        this IMongoDatabase collection,
        IClientSessionHandle clientSessionHandle)
    {
        return new MongoTransactionDatabase(collection, clientSessionHandle);
    }
}
