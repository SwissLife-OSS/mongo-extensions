using System;

namespace MongoDB.Extensions.Migration;

public class InvalidConfigurationException : Exception
{
    public InvalidConfigurationException(string message) : base(message) { }
}
