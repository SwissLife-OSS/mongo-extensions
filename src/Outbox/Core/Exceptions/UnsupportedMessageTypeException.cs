using System;

namespace SwissLife.MongoDB.Extensions.Outbox.Core.Exceptions
{
    public class UnsupportedMessageTypeException: Exception
    {
        public UnsupportedMessageTypeException(
            string messageTypeFullName, string consumerTypeFullName)
            : base($"Message of Type {messageTypeFullName} " +
                  $"is not supported by consumer {consumerTypeFullName}")
        {
            MessageTypeFullName = messageTypeFullName;
            ConsumerTypeFullName = consumerTypeFullName;
        }

        public string MessageTypeFullName { get; } 
        public string ConsumerTypeFullName { get; } 
    }
}
