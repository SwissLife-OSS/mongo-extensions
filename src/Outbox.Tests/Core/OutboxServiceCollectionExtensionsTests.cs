using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Extensions.Outbox.Core;
using Xunit;

namespace MongoDB.Extensions.Outbox.Tests.Core
{
    public class OutboxServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTransactionalOutbox_WithOptions_ShouldRegisterServiceAndHostedService()
        {
            //Arrange
            var services = new ServiceCollection();

            var options = new OutboxOptions();

            //Act
            services.AddTransactionalOutbox(options);

            //Assert
            services.Should().ContainSingle(s => s.ServiceType== typeof(OutboxOptions))
                .Which.Lifetime.Should().Be(ServiceLifetime.Singleton);
            services.Should().ContainSingle(s => s.ServiceType== typeof(IOutboxService))
                .Which.ImplementationType.Should().Be(typeof(OutboxService));
            services.Should().ContainSingle(s => s.ServiceType== typeof(IMessageProcessor))
                .Which.ImplementationType.Should().Be(typeof(MessageProcessor));
            services.Should().ContainSingle(s => s.ServiceType== typeof(IHostedService))
                .Which.ImplementationType.Should().Be(typeof(MessagesDeliveryService));
        }
    }
}
