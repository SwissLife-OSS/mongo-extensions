using System;

namespace MongoDB.Extensions.Migration;

public class BuilderNotInitializedException : Exception
{
    public BuilderNotInitializedException(string message) : base(message) { }
}
