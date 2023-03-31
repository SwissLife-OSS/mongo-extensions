using System;

namespace MongoDB.Extensions.Outbox.Core
{
    public class OutboxMessage
    {
        public OutboxMessage(
            Guid id, object payload, DateTime creationDate)
        {
            Id = id;
            Payload = payload;
            CreationDate = creationDate;
        }

        public Guid Id { get; set; }
        public object Payload { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
