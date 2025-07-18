using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Extensions.Context.Extensions;

#nullable enable

namespace MongoDB.Extensions.Context;

public class TypeObjectSerializer : ClassSerializerBase<object>, IHasDiscriminatorConvention
{
    private static readonly Dictionary<Type, bool> _allowedTypes = new();
    private static readonly HashSet<string> _allowedTypesByNamespaces = new();
    private static readonly HashSet<string> _allowedTypesByDependencies = new();
    private static readonly object _lock = new();

    public TypeObjectSerializer(ObjectSerializer? objectSerializer = null)
    {
        ObjectSerializer = objectSerializer ?? CreateObjectSerializer();

        DiscriminatorConvention = ObjectSerializer.GetDiscriminatorConvention();
    }

    public static IReadOnlyDictionary<Type, bool> AllowedTypes
        => _allowedTypes;

    public static IReadOnlyCollection<string> AllowedTypesByNamespaces
        => _allowedTypesByNamespaces;

    public static IReadOnlyCollection<string> AllowedTypesByDependencies
        => _allowedTypesByDependencies;

    public static bool IsTypeAllowed(Type type)
    {
        lock (_lock)
        {
            return ObjectSerializer.DefaultAllowedTypes.Invoke(type) ||
                   _allowedTypes.ContainsKey(type) ||
                   IsInAllowedNamespaces(type) ||
                   IsInAllowedDependencyTypes(type);
        }
    }

    public static void AddAllowedType<T>()
    {
        lock (_lock)
        {
            _allowedTypes.Add(typeof(T), true);
        }
    }

    public static void AddAllowedTypes(params Type[] allowedTypes)
    {
        lock (_lock)
        {
            foreach (Type allowedType in allowedTypes)
            {
                _allowedTypes.Add(allowedType, true);
            }
        }
    }

    public static void AddAllowedTypes(params string[] allowedNamespaces)
    {
        lock (_lock)
        {
            foreach (var allowedNamespace in allowedNamespaces)
            {
                _allowedTypesByNamespaces.Add(allowedNamespace);
            }
        }
    }

    public static void AddAllowedTypesOfAllDependencies(params string[] excludeNamespaces)
    {
        lock (_lock)
        {
            _allowedTypesByDependencies
                .UnionWith(DependencyTypesResolver.GetAllowedTypesByDependencies(excludeNamespaces));
        }
    }

    internal static void Clear()
    {
        lock (_lock)
        {
            _allowedTypes.Clear();
            _allowedTypesByNamespaces.Clear();
            _allowedTypesByDependencies.Clear();
        }
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
            _allowedTypes.Add(type, true);
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

        _allowedTypes.Add(type, isInDependencyTypes);

        return isInDependencyTypes;
    }

    public IDiscriminatorConvention DiscriminatorConvention { get; }

    public ObjectSerializer ObjectSerializer  { get; }

    public override object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return ObjectSerializer.Deserialize(context, args);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        ObjectSerializer.Serialize(context, args, value);
    }

    public override bool Equals(object? obj)
    {
        return ObjectSerializer.Equals(obj);
    }

    public override int GetHashCode()
    {
        return ObjectSerializer.GetHashCode();
    }

    private ObjectSerializer CreateObjectSerializer()
    {
        IDiscriminatorConvention objectDiscriminatorConvention =
            BsonSerializer.LookupDiscriminatorConvention(typeof(object));

        var serializer =
            BsonSerializer.LookupSerializer(typeof(Guid)) as GuidSerializer;

        GuidRepresentation guidRepresentation =
            serializer?.GuidRepresentation ?? GuidRepresentation.Unspecified;

        var objectSerializer =
            new ObjectSerializer(
                objectDiscriminatorConvention,
                guidRepresentation,
                type => IsTypeAllowed(type));

        return objectSerializer;
    }
}
