using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Extensions.Context;

internal class CustomObjectSerializer : ObjectSerializer
{
    private static readonly HashSet<Type> _knownTypes = new();

    public CustomObjectSerializer()
        : base(type => DefaultAllowedTypes(type) || _knownTypes.Contains(type))
    {
    }

    internal static void AddType<T>()
    {
        // TODO: Resolve all types from T
        _knownTypes.Add(typeof(T));
    }
}
