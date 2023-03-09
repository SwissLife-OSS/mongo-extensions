using System;

namespace MongoDB.Extensions.Context.Exceptions;

public class MissingAllowedTypesException : Exception
{
    public MissingAllowedTypesException() { }
    public MissingAllowedTypesException(string message) : base(message) { }
    public MissingAllowedTypesException(string message, Exception innerException) :
        base(message, innerException) { }
}
