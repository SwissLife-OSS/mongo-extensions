using MongoDB.Extensions.Context.Internal;
using Snapshooter.Xunit;
using Xunit;
using Xunit.Priority;

namespace MongoDB.Extensions.Context.AllowedTypes.Tests;

[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class TypeObjectSerializerTests
{
    [Fact]
    public void AddAllowedTypes_AddAllowedTypesOfAllDependencies_Success()
    {
        // Arrange
        TypeObjectSerializer.Clear();

        // Act
        TypeObjectSerializer.AddAllowedTypesOfAllDependencies();

        // Assert
        Snapshot.Match(new {
            TypeObjectSerializer.AllowedTypes,
            TypeObjectSerializer.AllowedTypesByNamespaces,
            TypeObjectSerializer.AllowedTypesByDependencies });
    }

    [Fact]
    public void AddAllowedTypes_AddAllowedTypesByNamespaces_Success()
    {
        // Arrange
        TypeObjectSerializer.Clear();

        // Act
        TypeObjectSerializer.AddAllowedTypes("Mongo", "SwissLife");

        // Assert
        Snapshot.Match(new {
            TypeObjectSerializer.AllowedTypes,
            TypeObjectSerializer.AllowedTypesByNamespaces,
            TypeObjectSerializer.AllowedTypesByDependencies });
    }

    [Fact]
    public void AddAllowedTypes_AddAllowedTypesByTypes_Success()
    {
        // Arrange
        TypeObjectSerializer.Clear();

        // Act
        TypeObjectSerializer.AddAllowedTypes(typeof(Foo), typeof(Bar));

        // Assert
        Snapshot.Match(new {
            TypeObjectSerializer.AllowedTypes,
            TypeObjectSerializer.AllowedTypesByNamespaces,
            TypeObjectSerializer.AllowedTypesByDependencies });
    }

    public class Bar
    {
        public int Id { get; set; }
        public string? BarName { get; set; }
    }

    public class Foo
    {
        public int Id { get; set; }
        public string? FooName { get; set; }
    }
}
