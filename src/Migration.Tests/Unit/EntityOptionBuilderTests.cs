using System;
using FluentAssertions;
using MongoDB.Extensions.Migration;
using Xunit;

namespace Migration.Tests.Unit;

public class EntityOptionBuilderTests
{
    [Fact]
    public void EntityOptionBuilder_AtVersionNotSet_SetToLatestVersion()
    {
        // Act
        EntityOption entityOption = new EntityOptionBuilder<TestEntity>()
            .WithMigration(new TestMigration1())
            .WithMigration(new TestMigration2())
            .Build();

        // Assert
        entityOption.CurrentVersion.Should().Be(2);
    }

    [Fact]
    public void EntityOptionBuilder_NoMigrationRegistered_Throws()
    {
        // Act
        Action action = () => new EntityOptionBuilder<TestEntity>().Build();

        // Assert
        action.Should().Throw<InvalidConfigurationException>();
    }

    [Fact]
    public void EntityOptionBuilder_GabInMigrationVersions_Throws()
    {
        // Act
        Action action = () => new EntityOptionBuilder<TestEntity>()
                .WithMigration(new TestMigration3())
                .WithMigration(new TestMigration1())
                .Build();

        // Assert
        action.Should().Throw<InvalidConfigurationException>();
    }

    [Fact]
    public void EntityOptionBuilder_AtVersionHasNoMigrationMigrationVersions_Throws()
    {
        // Act
        Action action = () => new EntityOptionBuilder<TestEntity>()
            .WithMigration(new TestMigration2())
            .WithMigration(new TestMigration1())
            .AtVersion(3)
            .Build();

        // Assert
        action.Should().Throw<InvalidConfigurationException>();
    }
}

record TestEntity(int Id) : IVersioned
{
    public int Version { get; set; }
}
