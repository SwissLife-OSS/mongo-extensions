using System;

namespace MongoDB.Extensions.Outbox.Core.Pipeline
{
    public class OutboxConsumerBuilder: OutboxBuilder
    {
        internal OutboxConsumerBuilder(
            OutboxBuildingPlan buildPlan,
            Type consumerType,
            Type messageType)
            : base(buildPlan)
        {
            ConsumerType = consumerType;
            MessageType = messageType;
        }

        public Type ConsumerType { get; }
        public Type MessageType { get; }
    }
}
