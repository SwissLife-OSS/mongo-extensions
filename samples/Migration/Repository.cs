using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Migration;

public class Repository
{
    private readonly IMongoCollection<Customer> _collection;

    public Repository(MongoClient client)
    {
        var database = client.GetDatabase("Example1");
        _collection = database.GetCollection<Customer>("customer");
    }

    public Task AddAsync(Customer customer) => _collection.InsertOneAsync(customer);

    public Task<Customer> GetAsync(string id) => _collection.AsQueryable()
        .SingleOrDefaultAsync(c => c.Id == id);
}
