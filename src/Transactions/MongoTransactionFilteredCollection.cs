using MongoDB.Driver;

namespace MongoDB.Extensions.Transactions;

public class MongoTransactionFilteredCollection<T>
    : MongoTransactionCollection<T>
    , IFilteredMongoCollection<T>
{
    private readonly IFilteredMongoCollection<T> _filteredCollection;

    public MongoTransactionFilteredCollection(
        IFilteredMongoCollection<T> filteredCollection)
        : base(filteredCollection)
    {
        _filteredCollection = filteredCollection;
    }

    public MongoTransactionFilteredCollection(
        IFilteredMongoCollection<T> filteredCollection,
        IClientSessionHandle clientSessionHandle)
        : base(filteredCollection, clientSessionHandle)
    {
        _filteredCollection = filteredCollection;
    }

    public FilterDefinition<T> Filter => _filteredCollection.Filter;
}
