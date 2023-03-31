using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SwissLife.MongoDB.Extensions.Outbox.Core
{
    internal class MessagesDeliveryService : BackgroundService
    {
        private readonly IOutboxRepository _outboxRepository;
        private readonly OutboxOptions _outboxOptions;
        private readonly ILogger<MessagesDeliveryService> _logger;
        private readonly IMessageProcessor _messageProcessor;

        public MessagesDeliveryService(
            IOutboxRepository outboxRepository,
            OutboxOptions outboxOptions,
            ILogger<MessagesDeliveryService> logger,
            IMessageProcessor messageProcessor)
        {
            _outboxRepository = outboxRepository;
            _outboxOptions = outboxOptions;
            _logger = logger;
            _messageProcessor = messageProcessor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            while (!stoppingToken.IsCancellationRequested)
            {
                Activity? activity = default;
                try
                {
                    await Task.Delay(
                        _outboxOptions.FallbackWorker.InboxReadInterval, stoppingToken);

                    await ProcessPendingMessages(stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogError(
                        $"Restart Process Pending Messages", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        $"Error while reading tracking entry message", ex);
                }
                finally
                {
                    activity?.Dispose();
                }
            }
        }

        protected virtual async Task ProcessPendingMessages(CancellationToken token)
        {
            IReadOnlyList<OutboxMessage> messages
                    = await _outboxRepository.GetPendingMessagesAsync(token);

            List<Task> pendingMessages = new List<Task>();

            foreach(OutboxMessage message in messages)
            {
                pendingMessages.Add(
                    _messageProcessor.TryProcessMessageAsync(message, token));
            }

            await Task.WhenAll(pendingMessages);
        }
    }
}
