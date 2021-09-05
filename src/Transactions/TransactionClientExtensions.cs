using MongoDB.Driver;

namespace MongoDB.Extensions.Transactions
{
    public static class TransactionClientExtensions
    {
        public static IMongoClient AsTransactionClient(this IMongoClient collection)
        {
            return new MongoTransactionClient(collection);
        }
    }
}
