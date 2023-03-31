using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SwissLife.MongoDB.Extensions.Outbox.Core
{
    internal class MessageProcessor: IMessageProcessor
    {
        private readonly OutboxOptions _outboxOptions;
        private readonly ILockProvider _lockProvider;
        private readonly IConsumptionFactory _consumptionFactory;
        private readonly IOutboxRepository _outboxRepository;
        private readonly ILogger<MessagesDeliveryService> _logger;

        public MessageProcessor(
            OutboxOptions outboxOptions,
            ILockProvider lockProvider,
            IConsumptionFactory consumptionFactory,
            IOutboxRepository outboxRepository,
            ILogger<MessagesDeliveryService> logger)
        {
            _outboxOptions = outboxOptions;
            _lockProvider = lockProvider;
            _consumptionFactory = consumptionFactory;
            _outboxRepository = outboxRepository;
            _logger = logger;
        }

        public async Task TryProcessMessageAsync(OutboxMessage message, CancellationToken token)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(_outboxOptions.Message.LockDuration);

            bool acquiredLock
                = await _lockProvider.AcquireMessageLockAsync(
                    message.Id, source.Token);

            if (!acquiredLock)
            {
                return;
            }

            await ConsumeMessage(message, source.Token);

            token.ThrowIfCancellationRequested();
        }

        private async Task ConsumeMessage(
            OutboxMessage message, CancellationToken token)
        {
            ITransactionSession session
                = await _outboxRepository.BeginTransactionAsync(token);

            try
            {
                IConsumption consumption
                    = _consumptionFactory.Create(message.Payload);

                await consumption.ConsumeAsync(token);

                await _outboxRepository.DeleteMessageAsync(
                    session, message.Id, token);
                await session.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Process Message Failed.", ex);
                throw;
            }
            finally
            {
                session.Dispose();
            }
        }
    }
}
