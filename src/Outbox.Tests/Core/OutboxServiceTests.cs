using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using MongoDB.Extensions.Outbox.Core;
using Xunit;

namespace MongoDB.Extensions.Outbox.Tests.Core
{
    public class OutboxServiceTests
    {
        [Fact]
        public async Task AddMessagesAsync_WithPayloads_ShouldAddMessagesToRepository()
        {
            //Arrange
            CancellationToken token = new CancellationTokenSource().Token;
            var payloads = new object[] { new object() };

            var transactionSection = new Mock<ITransactionSession>(MockBehavior.Strict);

            var repository = new Mock<IOutboxRepository>(MockBehavior.Strict);
            repository
                .Setup(r => r.AddMessagesAsync(
                    transactionSection.Object,
                    It.Is<IReadOnlyList<OutboxMessage>>(l => l.Count == payloads.Count()
                        && l.All(i => payloads.Contains(i.Payload))),
                    token))
                .Returns(Task.CompletedTask);

            var service = new OutboxService(repository.Object);

            //Act
            await service.AddMessagesAsync(transactionSection.Object, payloads, token);

            //Assert
            repository.VerifyAll();
        }
    }
}
