using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Extensions.Context.Extensions;

namespace MongoDB.Extensions.Context;

internal static class DependencyTypesResolver
{
    private static readonly HashSet<string> _notAllowedNames =
        new HashSet<string>{ "System", "Microsoft" };

    internal static HashSet<string> GetAllowedTypesByDependencies(string[] excludeNamespaces)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        IEnumerable<Assembly> allowedAssemblies = assemblies
            .Where(assembly => IsNamespaceAllowed(assembly.GetName().Name, excludeNamespaces));

        IEnumerable<string> namespaces = allowedAssemblies
            .SelectMany(a => a.GetTypes())
            .Select(type => type.GetRootNamespace())
            .Where(name => IsNamespaceAllowed(name, excludeNamespaces));

        return new HashSet<string>(namespaces);
    }

    private static bool IsNamespaceAllowed(string name, string [] excludeNamespaces)
    {
        if(string.IsNullOrEmpty(name))
        {
            return false;
        }

        return !_notAllowedNames.Concat(excludeNamespaces)
            .Any(entry => name.StartsWith(entry));
    }
}
