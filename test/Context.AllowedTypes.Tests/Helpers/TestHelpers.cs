using System.Collections.Generic;
using System.Linq;

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
                .Except(new[] { "Coverlet" })
                .OrderBy(x => x)
        };
    }
}
