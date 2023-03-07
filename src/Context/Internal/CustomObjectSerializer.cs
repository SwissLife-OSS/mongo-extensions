using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Extensions.Context;

internal class CustomObjectSerializer : ObjectSerializer
{
    private static readonly HashSet<string> _notAllowedNames =
        new HashSet<string>{ "System", "Microsoft" };

    private static readonly HashSet<string> _knownNamespaces = new();

    static CustomObjectSerializer()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        IEnumerable<Assembly> allowedAssemblies = assemblies
            .Where(assembly => IsNamespaceAllowed(assembly.GetName().Name));

        IEnumerable<string> namespaces = allowedAssemblies
            .SelectMany(a => a.GetTypes())
            .Select(type => GetNamespaceRoot(type.Namespace))
            .Where(name => IsNamespaceAllowed(name));

        _knownNamespaces = new HashSet<string>(namespaces);
    }

    public CustomObjectSerializer() : base(
        type => DefaultAllowedTypes(type) ||
        _knownNamespaces.Contains(GetNamespaceRoot(type.Namespace)))
    {
    }

    public IEnumerable<string> KnownNamespaces => _knownNamespaces.AsEnumerable();

    private static bool IsNamespaceAllowed(string name)
    {
        if(string.IsNullOrEmpty(name))
        {
            return false;
        }

        return !_notAllowedNames
            .Any(entry => name.StartsWith(entry));
    }

    private static string GetNamespaceRoot(string? fullNamespace)
    {
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
