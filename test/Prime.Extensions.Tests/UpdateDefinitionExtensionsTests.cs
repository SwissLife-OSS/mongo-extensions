using System;
using MongoDB.Driver;
using Snapshooter.Xunit;
using Xunit;

namespace MongoDB.Prime.Extensions.Tests
{
    public class UpdateDefinitionExtensionsTests
    {
        [Fact]
        public void ToDefinitionString_DefinitionStringNoIdent_Success()
        {
            // Arrange
            UpdateDefinition<Bar> filter =
                Builders<Bar>.Update.Set(field => field.Name, "Spain");

            // Act
            string filterString = filter.ToDefinitionString();

            // Assert
            Snapshot.Match(filterString);
        }

        [Fact]
        public void ToDefinitionString_DefinitionStringWithIdent_Success()
        {
            // Arrange
            UpdateDefinition<Bar> filter =
                Builders<Bar>.Update.Set(field => field.Id,
                    Guid.Parse("44752191-E10B-435A-A0E9-62E7F13D41CD"));

            // Act
            string filterString = filter.ToDefinitionString(indent: true);

            // Assert
            Snapshot.Match(filterString);
        }
    }
}
