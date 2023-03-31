using MongoDB.Extensions.Outbox.Core;
using MongoDB.Extensions.Outbox.Core.Pipeline;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class OutboxServiceCollectionExtensions
    {
        public static OutboxBuilder AddTransactionalOutbox(
            this IServiceCollection services,
            OutboxOptions options)
        {
            services
                .AddSingleton(options)
                .AddSingleton<IOutboxService, OutboxService>()
                .AddSingleton<IMessageProcessor, MessageProcessor>()
                .AddHostedService<MessagesDeliveryService>();

            var builder = new OutboxBuilder(services);

            return builder;
        }
    }
}
