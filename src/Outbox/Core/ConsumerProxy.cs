using System.Threading;
using System.Threading.Tasks;
using SwissLife.MongoDB.Extensions.Outbox.Core.Exceptions;

namespace SwissLife.MongoDB.Extensions.Outbox.Core
{
    public class ConsumerProxy<TMessage> : IConsumerProxy<TMessage>
        where TMessage : class
    {
        private readonly IConsumer<TMessage> _consumer;

        public ConsumerProxy(IConsumer<TMessage> consumer)
        {
            _consumer = consumer;
        }

        public Task ConsumeAsync(
            object message,
            CancellationToken token)
        {
            if (message is not TMessage tMessage)
            {
                throw new UnsupportedMessageTypeException(
                    message.GetType().FullName!,
                    _consumer.GetType().FullName!);
            }

            return _consumer.ConsumeAsync(tMessage, token);
        }
    }
}
