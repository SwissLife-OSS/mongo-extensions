using System.Collections.Generic;
using System.Linq;
using MongoDB.Extensions.Context.Internal;

namespace MongoDB.Extensions.Context.AllowedTypes.Tests.Helpers;

internal static class TestHelpers
{
    public static object GetTypeObjectSerializerContent()
    {
        return new
        {
            AllowedTypes = TypeObjectSerializer.AllowedTypes
                .Select(pair => new KeyValuePair<string?, bool>(pair.Key.FullName, pair.Value))
                .OrderBy(pair => pair.Key),
            AllowedTypesByNamespaces = TypeObjectSerializer.AllowedTypesByNamespaces
                .OrderBy(x => x),
            AllowedTypesByDependencies = TypeObjectSerializer.AllowedTypesByDependencies
                .OrderBy(x => x)
                .Except(new[] { "Coverlet" })
        };
    }
}
