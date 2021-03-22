using System.Text.Json;
using MongoDB.Bson;
using Snapshooter.Xunit;
using Xunit;

namespace MongoDB.Extensions.Context.Tests.Extensions
{
    public class BsonDocumentExtensionsTests
    {
        [Fact]
        public void ToJsonDocument_ConvertBsonToJsonDocument_ConvertedSuccessfully()
        {
            // Arrange
            var sut = new BsonDocument("profile", 0);

            // Act
            JsonDocument result = sut.ToJsonDocument();

            // Assert
            Snapshot.Match(resul----t);
        }
    }
}
