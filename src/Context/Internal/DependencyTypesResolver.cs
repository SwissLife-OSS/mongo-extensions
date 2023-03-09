using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Extensions.Context.Extensions;

namespace MongoDB.Extensions.Context;

internal class DependencyTypesResolver
{
    private static readonly HashSet<string> _notAllowedNames =
        new HashSet<string>{ "System", "Microsoft" };

    public static HashSet<string> GetAllowedTypesByDependencies()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        IEnumerable<Assembly> allowedAssemblies = assemblies
            .Where(assembly => IsNamespaceAllowed(assembly.GetName().Name));

        IEnumerable<string> namespaces = allowedAssemblies
            .SelectMany(a => a.GetTypes())
            .Select(type => type.GetRootNamespace())
            .Where(name => IsNamespaceAllowed(name));

        return new HashSet<string>(namespaces);
    }

    private static bool IsNamespaceAllowed(string name)
    {
        if(string.IsNullOrEmpty(name))
        {
            return false;
        }

        return !_notAllowedNames
            .Any(entry => name.StartsWith(entry));
    }
}
