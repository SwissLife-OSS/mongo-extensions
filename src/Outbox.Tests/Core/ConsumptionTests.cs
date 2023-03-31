using System.Threading;
using System.Threading.Tasks;
using Moq;
using MongoDB.Extensions.Outbox.Core;
using Xunit;

namespace MongoDB.Extensions.Outbox.Tests.Core
{
    public class ConsumptionTests
    {
        [Fact]
        public async Task ConsumeAsync_WithMessage_ShouldForwardToProxy()
        {
            //Arrange
            var message = new DummyMessage();
            CancellationToken token = new CancellationTokenSource().Token;

            var proxy = new Mock<IConsumerProxy<DummyMessage>>(MockBehavior.Strict);
            proxy
                .Setup(c => c.ConsumeAsync(message, token))
                .Returns(Task.CompletedTask);

            var consumption = new Consumption<DummyMessage>(proxy.Object, message);

            //Act
            await consumption.ConsumeAsync(token);

            //Assert
            proxy.VerifyAll();
        }

        public class DummyMessage
        {
        }
    }
}
