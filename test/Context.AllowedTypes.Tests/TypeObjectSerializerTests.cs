using MongoDB.Extensions.Context.AllowedTypes.Tests.Helpers;
using Snapshooter.Xunit;
using Xunit;

namespace MongoDB.Extensions.Context.AllowedTypes.Tests;

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
        Snapshot.Match(TestHelpers.GetTypeObjectSerializerContent(),
            options => options.Assert(fieldOption =>
                Assert.Contains("MongoDB", fieldOption
                    .Fields<string>("AllowedTypesByDependencies[*]"))));
    }

    [Fact]
    public void AddAllowedTypes_AddAllowedTypesByNamespaces_Success()
    {
        // Arrange
        TypeObjectSerializer.Clear();

        // Act
        TypeObjectSerializer.AddAllowedTypes("Mongo", "SwissLife");

        // Assert
        Snapshot.Match(TestHelpers.GetTypeObjectSerializerContent());
    }

    [Fact]
    public void AddAllowedTypes_AddAllowedTypesByTypes_Success()
    {
        // Arrange
        TypeObjectSerializer.Clear();

        // Act
        TypeObjectSerializer.AddAllowedTypes(typeof(Foo), typeof(Bar));

        // Assert
        Snapshot.Match(TestHelpers.GetTypeObjectSerializerContent());
    }

    [Fact]
    public void IsTypeAllowed_InAllowedTypes_True()
    {
        // Arrange
        TypeObjectSerializer.Clear();
        TypeObjectSerializer.AddAllowedTypes(typeof(Foo));

        // Act
        bool isAllowed = TypeObjectSerializer.IsTypeAllowed(typeof(Foo));

        // Assert
        Assert.True(isAllowed);
        Snapshot.Match(TestHelpers.GetTypeObjectSerializerContent());
    }

    [Fact]
    public void IsTypeAllowed_InAllowedTypes_False()
    {
        // Arrange
        TypeObjectSerializer.Clear();
        TypeObjectSerializer.AddAllowedTypes(typeof(Foo));

        // Act
        bool isAllowed = TypeObjectSerializer.IsTypeAllowed(typeof(Bar));

        // Assert
        Assert.False(isAllowed);
        Snapshot.Match(TestHelpers.GetTypeObjectSerializerContent());
    }

    [Fact]
    public void IsTypeAllowed_InAllowedNamespaces_True()
    {
        // Arrange
        TypeObjectSerializer.Clear();
        TypeObjectSerializer.AddAllowedTypes("MongoDB.Extensions.Context.AllowedTypes.Tests");

        // Act
        bool isAllowed = TypeObjectSerializer.IsTypeAllowed(typeof(Foo));

        // Assert
        Assert.True(isAllowed);
        Snapshot.Match(TestHelpers.GetTypeObjectSerializerContent());
    }

    [Fact]
    public void IsTypeAllowed_PartIsInAllowedNamespaces_True()
    {
        // Arrange
        TypeObjectSerializer.Clear();
        TypeObjectSerializer.AddAllowedTypes("MongoDB.Extensions.Context");

        // Act
        bool isAllowed = TypeObjectSerializer.IsTypeAllowed(typeof(Foo));

        // Assert
        Assert.True(isAllowed);
        Snapshot.Match(TestHelpers.GetTypeObjectSerializerContent());
    }

    [Fact]
    public void IsTypeAllowed_PartIsInAllowedNamespacesCaseInsensitive_True()
    {
        // Arrange
        TypeObjectSerializer.Clear();
        TypeObjectSerializer.AddAllowedTypes("MONGODB.EXTENSIONS");

        // Act
        bool isAllowed = TypeObjectSerializer.IsTypeAllowed(typeof(Foo));

        // Assert
        Assert.True(isAllowed);
        Snapshot.Match(TestHelpers.GetTypeObjectSerializerContent());
    }

    [Fact]
    public void IsTypeAllowed_PartIsNotInAllowedNamespaces_False()
    {
        // Arrange
        TypeObjectSerializer.Clear();
        TypeObjectSerializer.AddAllowedTypes("MongoDBs.Context");

        // Act
        bool isAllowed = TypeObjectSerializer.IsTypeAllowed(typeof(Foo));

        // Assert
        Assert.False(isAllowed);
        Snapshot.Match(TestHelpers.GetTypeObjectSerializerContent());
    }

    [Fact]
    public void IsTypeAllowed_InAllowedTypesInDependencies_True()
    {
        // Arrange
        TypeObjectSerializer.Clear();
        TypeObjectSerializer.AddAllowedTypesOfAllDependencies();

        // Act
        bool isAllowed = TypeObjectSerializer.IsTypeAllowed(typeof(Foo));

        // Assert
        Assert.True(isAllowed);
        Snapshot.Match(TestHelpers.GetTypeObjectSerializerContent(),
            options => options.Assert(fieldOption =>
                Assert.Contains("MongoDB", fieldOption
                    .Fields<string>("AllowedTypesByDependencies[*]"))));
    }
}
