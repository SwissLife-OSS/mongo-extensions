using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SwissLife.MongoDB.Extensions.Outbox.Core;
using Xunit;

namespace SwissLife.MongoDB.Extensions.Outbox.Tests.Core
{
    public class ConsumptionFactoryTests
    {
        [Fact]
        public void Create_ForSupportedMessage_ShouldReturnConsumptionWithProxy()
        {
            //Arrange
            var proxy = new DummyProxy();

            var services = new ServiceCollection();
            IServiceProvider provider = services.BuildServiceProvider();

            Func<IServiceProvider, DummyProxy> proxyFact = (IServiceProvider p) => proxy;

            var factory = new ConsumptionFactory(
                provider, new Dictionary<Type, Func<IServiceProvider, IConsumerProxy>>
                {
                    { typeof(DummyMessage),  proxyFact }
                });

            //Act
            IConsumption job = factory.Create(new DummyMessage());

            //Assert
            job.Should().BeOfType<Consumption<DummyMessage>>()
                .Subject.ConsumerProxy.Should().Be(proxy);
        }

        [Fact]
        public void Create_ForUnsupportedMessage_ShouldThrow()
        {
            //Arrange
            var services = new ServiceCollection();
            IServiceProvider provider = services.BuildServiceProvider();

            Func<IServiceProvider, DummyProxy> proxyFact = (IServiceProvider p)
                => new DummyProxy();

            var factory = new ConsumptionFactory(
                provider, new Dictionary<Type, Func<IServiceProvider, IConsumerProxy>>
                {
                    { typeof(DummyMessage),  proxyFact }
                });

            //Act
            Func<IConsumption> tryGetConsumption = () => factory.Create(new object());

            //Assert
            tryGetConsumption.Should().Throw<KeyNotFoundException>();
        }

        public class DummyMessage
        {
        }

        public class DummyProxy : IConsumerProxy
        {
            public Task ConsumeAsync(object message, CancellationToken token)
            {
                throw new NotImplementedException();
            }
        }
    }
}
