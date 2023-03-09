using System;

namespace MongoDB.Extensions.Context.Extensions;

internal static class TypeExtensions
{
    public static string GetRootNamespace(this Type? type)
    {
        string? fullNamespace = type?.Namespace;

        if(string.IsNullOrEmpty(fullNamespace))
        {
            return string.Empty;
        }

        ReadOnlySpan<char> namespaceSpan = fullNamespace.AsSpan();

        int offset = namespaceSpan.IndexOf('.');

        if(offset <= 0)
        {
            return fullNamespace;
        }

        return namespaceSpan
            .Slice(0, offset)
            .ToString();
    }
}
