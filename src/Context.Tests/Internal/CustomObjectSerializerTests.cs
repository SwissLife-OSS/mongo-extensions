using System.Collections.Generic;
using System.Linq;
using Snapshooter.Xunit;
using Xunit;

namespace MongoDB.Extensions.Context.Tests.Internal;

public class CustomObjectSerializerTests
{
    [Fact]
    public void Constructor_RegisterAllKnownNamespaces_InitializeSuccessful()
    {
        // Arrange
        var customObjectSerializer = new CustomObjectSerializer();

        // Act
        IEnumerable<string> knownNamespaces =
            customObjectSerializer.KnownNamespaces;
                
        // Assert
        Snapshot.Match(knownNamespaces.OrderBy(x => x));
    }
}
