using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoDB.Extensions.Session;

internal class MongoClientSessionFactory : ISessionFactory
{
    private readonly IMongoClient _client;

    public MongoClientSessionFactory(IMongoClient client)
    {
        _client = client;
    }
    
    public async ValueTask<IClientSessionHandle> CreateSessionAsync(
        CancellationToken cancellationToken)
    {
        return await _client
            .StartSessionAsync(cancellationToken: cancellationToken);
    }
}
