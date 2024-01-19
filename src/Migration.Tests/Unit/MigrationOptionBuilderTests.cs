using System;
using FluentAssertions;
using MongoDB.Extensions.Migration;
using Xunit;

namespace Migration.Tests.Unit;

public class MigrationOptionBuilderTests
{
    [Fact]
    public void MigrationOptionBuilder_RegisterEntityTwice_Throws()
    {
        // Arrange
        var builder = new MigrationOptionBuilder();
        builder.ForEntity<TestEntity>(b => b.WithMigration(new TestMigration()));

        // Act
        Action action = () => builder.ForEntity<TestEntity>(b => b.WithMigration(new TestMigration()));

        // Assert
        action.Should().Throw<InvalidConfigurationException>();
    }
}
