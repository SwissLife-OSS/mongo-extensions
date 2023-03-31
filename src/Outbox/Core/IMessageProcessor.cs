using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Extensions.Outbox.Core
{
    public interface IMessageProcessor
    {
        Task TryProcessMessageAsync(OutboxMessage message, CancellationToken token);
    }
}
