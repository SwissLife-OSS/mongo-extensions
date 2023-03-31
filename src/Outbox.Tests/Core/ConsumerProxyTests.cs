using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MongoDB.Extensions.Outbox.Core;
using MongoDB.Extensions.Outbox.Core.Exceptions;
using Xunit;

namespace MongoDB.Extensions.Outbox.Tests.Core
{
    public class ConsumerProxyTests
    {
        [Fact]
        public async Task ConsumeAsync_WithValidMessageType_ShouldAcceptConsumption()
        {
            //Arrange
            var message = new DummyMessage();
            CancellationToken token = new CancellationTokenSource().Token;

            var consumer = new Mock<IConsumer<DummyMessage>>(MockBehavior.Strict);
            consumer
                .Setup(c => c.ConsumeAsync(message, token))
                .Returns(Task.CompletedTask);

            var proxy = new ConsumerProxy<DummyMessage>(consumer.Object);

            //Act
            await proxy.ConsumeAsync(message, token);

            //Assert
            consumer.VerifyAll();
        }

        [Fact]
        public async Task ConsumeAsync_WithUnsupportedMessageType_ShouldThrowException()
        {
            //Arrange
            var message = new object();
            CancellationToken token = new CancellationTokenSource().Token;

            var consumer = new Mock<IConsumer<DummyMessage>>(MockBehavior.Strict);

            var proxy = new ConsumerProxy<DummyMessage>(consumer.Object);

            //Act
            Func<Task> tryConsume = async () => await proxy.ConsumeAsync(message, token);

            //Assert
            await tryConsume.Should().ThrowAsync<UnsupportedMessageTypeException>();
        }

        public class DummyMessage
        {
        }
    }
}
