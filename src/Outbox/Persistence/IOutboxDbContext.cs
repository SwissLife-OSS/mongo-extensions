using MongoDB.Extensions.Context;

namespace SwissLife.MongoDB.Extensions.Outbox.Persistence
{
    public interface IOutboxDbContext: IMongoDbContext
    {
    }
}
