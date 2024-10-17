using System.Collections.Generic;
using System.Linq;
using Snapshooter.Xunit;
using Xunit;

namespace MongoDB.Extensions.Context.Tests.Internal;

[Collection("Sequential")]
public class DependencyTypesResolverTests
{
    [Fact(Skip = "Flaky test")]
    public void GetAllowedTypesByDependencies_All_Successful()
    {
        // Arrange

        // Act
        IEnumerable<string> knownNamespaces = DependencyTypesResolver
            .GetAllowedTypesByDependencies(new[] { "Coverlet" })
            .OrderBy(x => x);

        // Assert
        Snapshot.Match(knownNamespaces);
    }
}
