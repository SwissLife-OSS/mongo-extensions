using MongoDB.Extensions.Context;

namespace MongoDB.Extensions.Outbox.Persistence
{
    public interface IOutboxDbContext: IMongoDbContext
    {
    }
}
