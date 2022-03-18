using MongoDB.Driver;

#pragma warning disable 618

namespace MongoDB.Extensions.Transactions
{
    public class MongoTransactionFilteredCollection<T> : MongoTransactionCollection<T>, IFilteredMongoCollection<T>
    {
        private readonly IFilteredMongoCollection<T> _filteredCollection;

        public FilterDefinition<T> Filter => _filteredCollection.Filter;

        public MongoTransactionFilteredCollection(IFilteredMongoCollection<T> filteredCollection)
            : base(filteredCollection)
        {
            _filteredCollection = filteredCollection;
        }

    }
}
