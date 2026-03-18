using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;
using MongoDB.Extensions.Migration;
using Xunit;

namespace Migration.Tests.Unit;

public class MigrationRunnerTests
{
    [Fact]
    public void MigrationRunner_MigrationUpThrows_Catches()
    {
        // Arrange
        var runner = new MigrationRunner<TestEntity>(new EntityContext(
            new EntityOptionBuilder<TestEntity>().WithMigration(new TestMigration())
                .Build(),
            new NullLoggerFactory()));
        var document = new Dictionary<string, object> { ["Version"] = 1, ["_id"] = 1 };

        // Act
        Action action = () => runner.Run(new BsonDocument(document));

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void MigrationRunner_MigrationDownThrows_Catches()
    {
        // Arrange
        var runner = new MigrationRunner<TestEntity>(new EntityContext(
            new EntityOptionBuilder<TestEntity>().WithMigration(new TestMigration()).AtVersion(0)
                .Build(),
            new NullLoggerFactory()));
        var document = new Dictionary<string, object> { ["Version"] = 1, ["_id"] = 1 };

        // Act
        Action action = () => runner.Run(new BsonDocument(document));
        

        // Assert
        action.Should().NotThrow();
    }

}
