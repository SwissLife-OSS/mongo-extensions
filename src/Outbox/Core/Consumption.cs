using System.Threading;
using System.Threading.Tasks;

namespace SwissLife.MongoDB.Extensions.Outbox.Core
{
    public class Consumption<TMessage> : IConsumption
        where TMessage : class
    {
        private readonly TMessage _message;

        public Consumption(
            IConsumerProxy consumerProxy,
            TMessage message)
        {
            ConsumerProxy = consumerProxy;
            _message = message;
        }

        internal IConsumerProxy ConsumerProxy { get; set; }

        public Task ConsumeAsync(
            CancellationToken token)
        {
            return ConsumerProxy.ConsumeAsync(_message, token);
        }
    }
}
