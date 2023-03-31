using System;

namespace SwissLife.MongoDB.Extensions.Outbox.Persistence
{
    public class MessageLock
    {
        public MessageLock(Guid id, DateTime lockedAt)
        {
            Id = id;
            LockedAt = lockedAt;
        }

        public Guid Id { get; set; }
        public DateTime LockedAt { get; set; }
    }
}
