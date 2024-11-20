using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Extensions.Context.Extensions;

#nullable enable

namespace MongoDB.Extensions.Context.Internal;

internal class TypeObjectSerializer : ClassSerializerBase<object>, IHasDiscriminatorConvention
{
    private readonly ObjectSerializer _objectSerializer;
    private static readonly ConcurrentDictionary<Type, bool> _allowedTypes = new();
    private static readonly HashSet<string> _allowedTypesByNamespaces = new();
    private static readonly HashSet<string> _allowedTypesByDependencies = new();

    public TypeObjectSerializer()
    {
        _objectSerializer = new ObjectSerializer(type => IsTypeAllowed(type));
        DiscriminatorConvention = _objectSerializer.GetDiscriminatorConvention();
    }

    public static IReadOnlyDictionary<Type, bool> AllowedTypes
        => _allowedTypes;

    public static IReadOnlyCollection<string> AllowedTypesByNamespaces
        => _allowedTypesByNamespaces;

    public static IReadOnlyCollection<string> AllowedTypesByDependencies
        => _allowedTypesByDependencies;

    public static bool IsTypeAllowed(Type type)
    {
        return ObjectSerializer.DefaultAllowedTypes.Invoke(type) ||
               _allowedTypes.ContainsKey(type) ||
               IsInAllowedNamespaces(type) ||
               IsInAllowedDependencyTypes(type);
    }

    public static void AddAllowedType<T>()
    {
        _allowedTypes.TryAdd(typeof(T), true);
    }

    public static void AddAllowedTypes(params Type[] allowedTypes)
    {
        foreach (Type allowedType in allowedTypes)
        {
            _allowedTypes.TryAdd(allowedType, true);
        }
    }

    public static void AddAllowedTypes(params string[] allowedNamespaces)
    {
        foreach (string allowedNamespace in allowedNamespaces)
        {
            _allowedTypesByNamespaces.Add(allowedNamespace);
        }
    }

    public static void AddAllowedTypesOfAllDependencies(params string[] excludeNamespaces)
    {
        _allowedTypesByDependencies
            .UnionWith(DependencyTypesResolver.GetAllowedTypesByDependencies(excludeNamespaces));
    }

    internal static void Clear()
    {
        _allowedTypes.Clear();
        _allowedTypesByNamespaces.Clear();
        _allowedTypesByDependencies.Clear();
    }

    private static bool IsInAllowedNamespaces(Type type)
    {
        if (type.Namespace is null)
        {
            return false;
        }

        var isInAllowedNamespaces = IsAllowedNameSpacePart(type);

        if (isInAllowedNamespaces)
        {
            _allowedTypes.TryAdd(type, true);
        }

        return isInAllowedNamespaces;
    }

    private static bool IsAllowedNameSpacePart(Type type)
    {
        foreach (string allowedNamespace in _allowedTypesByNamespaces)
        {
            if (string.IsNullOrEmpty(type.Namespace))
            {
                return false;
            }

            if (type.Namespace.StartsWith(allowedNamespace,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInAllowedDependencyTypes(Type type)
    {
        bool isInDependencyTypes = _allowedTypesByDependencies
            .Contains(type.GetRootNamespace());

        _allowedTypes.TryAdd(type, isInDependencyTypes);

        return isInDependencyTypes;
    }

    public IDiscriminatorConvention DiscriminatorConvention { get; }

    public override object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return _objectSerializer.Deserialize(context, args);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        _objectSerializer.Serialize(context, args, value);
    }

    public override bool Equals(object? obj)
    {
        return _objectSerializer.Equals(obj);
    }

    public override int GetHashCode()
    {
        return _objectSerializer.GetHashCode();
    }
}
