using System;
using System.Collections.Generic;

namespace MongoDB.Extensions.Outbox.Core
{
    public class ConsumptionFactory : IConsumptionFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IReadOnlyDictionary<Type, Func<IServiceProvider, IConsumerProxy>>
            _consumerFactories;

        public ConsumptionFactory(
            IServiceProvider serviceProvider,
            IReadOnlyDictionary<Type, Func<IServiceProvider, IConsumerProxy>> consumerFactories)
        {
            _serviceProvider = serviceProvider;
            _consumerFactories = consumerFactories;
        }

        public IConsumption Create<TMessage>(TMessage message)
            where TMessage : class
        {
            IConsumerProxy proxy
                = _consumerFactories[message.GetType()](_serviceProvider);

            return new Consumption<TMessage>(proxy, message);
        }
    }
}
