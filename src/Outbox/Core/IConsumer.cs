using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Extensions.Outbox.Core
{
    public interface IConsumer<TMessage>
        where TMessage: class
    {
        Task ConsumeAsync(TMessage payload, CancellationToken token);
    }
}
