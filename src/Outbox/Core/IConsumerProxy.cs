using System.Threading;
using System.Threading.Tasks;

namespace SwissLife.MongoDB.Extensions.Outbox.Core
{
    public interface IConsumerProxy
    {
        Task ConsumeAsync(object message, CancellationToken token);
    }

    public interface IConsumerProxy<TMessage>: IConsumerProxy
    {
    }
}
