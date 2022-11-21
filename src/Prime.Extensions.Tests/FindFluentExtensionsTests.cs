using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using Snapshooter.Xunit;
using Squadron;
using System.Threading.Tasks;
using Xunit;

namespace MongoDB.Prime.Extensions.Tests
{
    public class FindFluentExtensionsTests : IClassFixture<MongoResource>
    {
        private readonly IMongoDatabase _mongoDatabase;

        public FindFluentExtensionsTests(MongoResource mongoResource)
        {
            _mongoDatabase = mongoResource.CreateDatabase();
        }

        [Fact]
        public void PrintQuery_PrintOneSingleQuery_OriginalMongoDbQueryPrinted()
        {
            // Arrange
            IMongoCollection<Bar> barCollection =
                _mongoDatabase.GetCollection<Bar>();

            // Act
            string mongodbQuery = barCollection
                .Find<Bar>(bar => bar.Name == "Bar1" || bar.Value == "1234")
                .Limit(5)
                .PrintQuery();

            // Assert
            Snapshot.Match(mongodbQuery);
        }
    }
}
