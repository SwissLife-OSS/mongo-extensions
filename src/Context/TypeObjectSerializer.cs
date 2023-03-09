using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Extensions.Context.Extensions;

namespace MongoDB.Extensions.Context.Internal;

public class TypeObjectSerializer : ObjectSerializer
{
    private static readonly HashSet<Type> _allowedTypes = new ();
    private static readonly HashSet<string> _allowedTypesByNamespaces = new ();
    private static readonly HashSet<string> _allowedTypesByDependencies = new ();

    public TypeObjectSerializer() : base(
        type => DefaultAllowedTypes(type) ||
        _allowedTypes.Contains(type) ||
        _allowedTypesByNamespaces.Contains(type.Namespace) ||
        _allowedTypesByDependencies.Contains(type.GetRootNamespace()))
    {
    }

    public static IReadOnlyCollection<Type> AllowedTypes
        => _allowedTypes;

    public static IReadOnlyCollection<string> AllowedTypesByNamespaces
        => _allowedTypesByNamespaces;

    public static IReadOnlyCollection<string> AllowedTypesByDependencies
        => _allowedTypesByDependencies;

    public static void AddAllowedType<T>()
    {
        _allowedTypes.Add(typeof(T));
    }

    public static void AddAllowedTypes(params Type[] allowedTypes)
    {
        foreach (Type allowedType in allowedTypes)
        {
            _allowedTypes.Add(allowedType);
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
}
