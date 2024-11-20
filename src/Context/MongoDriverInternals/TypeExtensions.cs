using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MongoDB.Extensions.Context;

internal static class TypeExtensions
{
    public static bool IsAnonymousType(this Type type)
    {
        // don't test for too many things in case implementation details change in the future
        return
            type.GetCustomAttributes(false).Any(x => x is CompilerGeneratedAttribute) &&
            type.IsGenericType &&
            type.Name.Contains("Anon"); // don't check for more than "Anon" so it works in mono also
    }
}