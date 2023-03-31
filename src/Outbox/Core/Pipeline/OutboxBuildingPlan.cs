using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace SwissLife.MongoDB.Extensions.Outbox.Core.Pipeline
{
    public class OutboxBuildingPlan
    {
        public OutboxBuildingPlan(IServiceCollection services)
        {
            Services = services;
            ConsumerBuilders = new List<OutboxConsumerBuilder>();

            services.AddSingleton<IConsumptionFactory>(p =>
            {
                var factories = ConsumerBuilders
                    .ToDictionary(
                        b => b.MessageType,
                        b => GetConsumerProxyFactory(b.ConsumerType));

                return new ConsumptionFactory(p, factories);
            });
        }

        public IServiceCollection Services { get; }
        internal List<OutboxConsumerBuilder> ConsumerBuilders { get; }

        private static Func<IServiceProvider, IConsumerProxy> GetConsumerProxyFactory(
            Type consumerProxyType)
        {
            return p => GetConsumerProxy(p, consumerProxyType);
        }

        private static IConsumerProxy GetConsumerProxy(
            IServiceProvider p,
            Type consumerProxyType)
        {
            IConsumerProxy proxy
                = (IConsumerProxy)p.GetRequiredService(consumerProxyType);

            return proxy;
        }
    }
}
