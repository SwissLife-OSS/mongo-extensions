using Microsoft.Extensions.DependencyInjection;

namespace MongoDB.Extensions.Outbox.Core.Pipeline
{
    public class OutboxBuilder
    {
        private readonly OutboxBuildingPlan _buildPlan;

        protected OutboxBuilder(OutboxBuildingPlan buildPlan)
        {
            _buildPlan = buildPlan;
        }

        public IServiceCollection Services => _buildPlan.Services;

        internal OutboxBuilder(IServiceCollection services)
        {
            _buildPlan = new OutboxBuildingPlan(services);
        }

        public OutboxBuilder AddConsumer<TConsumer, TMessage>()
            where TConsumer : class, IConsumer<TMessage>
            where TMessage : class
        {
            _buildPlan.Services.AddTransient<IConsumer<TMessage>, TConsumer>();
            _buildPlan.Services.AddTransient<
                IConsumerProxy<TMessage>, ConsumerProxy<TMessage>>();

            _buildPlan.ConsumerBuilders.Add(
                new OutboxConsumerBuilder(
                    _buildPlan,
                    consumerType: typeof(IConsumerProxy<TMessage>),
                    messageType: typeof(TMessage)));

            return this;
        }
    }
}
