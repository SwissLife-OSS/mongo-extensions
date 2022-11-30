using MongoDB.Driver;

namespace MongoDB.Extensions.Transactions;

public static class TransactionCollectionExtensions
{
    public static IMongoCollection<T> AsTransactionCollection<T>(
        this IMongoCollection<T> collection)
    {
        return new MongoTransactionCollection<T>(collection);
    }

    public static IMongoCollection<T> AsTransactionCollection<T>(
        this IMongoCollection<T> collection,
        IClientSessionHandle clientSessionHandle)
    {
        return new MongoTransactionCollection<T>(collection, clientSessionHandle);
    }

    public static IFilteredMongoCollection<T> AsTransactionCollection<T>(
        this IFilteredMongoCollection<T> collection)
    {
        return new MongoTransactionFilteredCollection<T>(collection);
    }

    public static IFilteredMongoCollection<T> AsTransactionCollection<T>(
        this IFilteredMongoCollection<T> collection,
        IClientSessionHandle clientSessionHandle)
    {
        return new MongoTransactionFilteredCollection<T>(collection, clientSessionHandle);
    }
}
