using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Extensions.Context.Extensions;

namespace MongoDB.Extensions.Context.Internal;

public class TypeObjectSerializer : ObjectSerializer
{
    private static readonly ConcurrentDictionary<Type, bool> _allowedTypes = new ();
    private static readonly HashSet<string> _allowedTypesByNamespaces = new ();
    private static readonly HashSet<string> _allowedTypesByDependencies = new ();

    public TypeObjectSerializer() : base(type => IsTypeAllowed(type))
    {
    }

    public static IReadOnlyDictionary<Type, bool> AllowedTypes
        => _allowedTypes;

    public static IReadOnlyCollection<string> AllowedTypesByNamespaces
        => _allowedTypesByNamespaces;

    public static IReadOnlyCollection<string> AllowedTypesByDependencies
        => _allowedTypesByDependencies;

    public static bool IsTypeAllowed(Type type)
    {
        return DefaultAllowedTypes(type) ||
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

    public static void AddAllowedTypesOfAllDependencies()
    {
        _allowedTypesByDependencies
            .UnionWith(DependencyTypesResolver.GetAllowedTypesByDependencies());
    }

    internal static void Clear()
    {
        _allowedTypes.Clear();
        _allowedTypesByNamespaces.Clear();
        _allowedTypesByDependencies.Clear();
    }

    private static bool IsInAllowedNamespaces(Type type)
    {
        if(type.Namespace is null)
        {
            return false;
        }

        bool isInAllowedNamespaces = _allowedTypesByNamespaces
            .Contains(type.Namespace);

        if(isInAllowedNamespaces)
        {
            _allowedTypes.TryAdd(type, true);
        }

        return isInAllowedNamespaces;
    }

    private static bool IsInAllowedDependencyTypes(Type type)
    {
        bool isInDependencyTypes = _allowedTypesByDependencies
            .Contains(type.GetRootNamespace());

        _allowedTypes.TryAdd(type, isInDependencyTypes);

        return isInDependencyTypes;
    }
}
