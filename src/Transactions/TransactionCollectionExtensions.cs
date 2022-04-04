using MongoDB.Driver;

namespace MongoDB.Extensions.Transactions
{
    public static class TransactionCollectionExtensions
    {
        public static IMongoCollection<T> AsTransactionCollection<T>(
            this IMongoCollection<T> collection)
        {
            return new MongoTransactionCollection<T>(collection);
        }

        public static IFilteredMongoCollection<T> AsTransactionCollection<T>(
            this IFilteredMongoCollection<T> collection)
        {
            return new MongoTransactionFilteredCollection<T>(collection);
        }
    }
}
