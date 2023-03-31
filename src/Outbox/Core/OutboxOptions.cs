using System;

namespace MongoDB.Extensions.Outbox.Core
{
    public class OutboxOptions
    {
        public OutboxFallBackWorkerOptions FallbackWorker { get; set; }

        public MessageOptions Message { get; set; }
    }

    public class OutboxOptions<TPersistenceOptions>: OutboxOptions
        where TPersistenceOptions : class
    {
        public TPersistenceOptions Persistence { get; set; }
    }

    public class OutboxFallBackWorkerOptions
    {
        public TimeSpan MessageMinimumAge { get; set; }
        public TimeSpan InboxReadInterval { get; set; }
    }

    public class MessageOptions
    {
        public TimeSpan LockDuration { get; set; }
    }
}
