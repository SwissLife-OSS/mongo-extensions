using System.Collections.Generic;
using System.Linq;
using Snapshooter.Xunit;
using Xunit;

namespace MongoDB.Extensions.Context.Tests.Internal;

public class DependencyTypesResolverTests
{
    [Fact]
    public void GetAllowedTypesByDependencies_All_Successful()
    {
        // Arrange

        // Act
        IEnumerable<string> knownNamespaces =
            DependencyTypesResolver.GetAllowedTypesByDependencies();

        // Assert
        Snapshot.Match(knownNamespaces.OrderBy(x => x));
    }
}
