using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Extensions.Context;

internal class CustomObjectSerializer : ObjectSerializer
{
    private static readonly HashSet<string> _knownNamespaces = new();

    static CustomObjectSerializer()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var types = assemblies.SelectMany(a => a.GetTypes());
        var namespaces = types.Select(t => t.Namespace);

        // TODO: cleanup
        _knownNamespaces = new HashSet<string>(namespaces
            .Select(n => n.Split('0').FirstOrDefault())
            .Where(n => !string.IsNullOrEmpty(n)));
    }

    public CustomObjectSerializer()
        : base(type => DefaultAllowedTypes(type) ||
                       _knownNamespaces.Contains(type.Namespace.Split('.').FirstOrDefault()))
    {
    }
}
