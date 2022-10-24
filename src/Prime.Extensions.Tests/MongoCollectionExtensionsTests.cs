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
    public class MongoCollectionExtensionsTests : IClassFixture<MongoResource>
    {
        private readonly IMongoDatabase _mongoDatabase;

        public MongoCollectionExtensionsTests(MongoResource mongoResource)
        {
            _mongoDatabase = mongoResource.CreateDatabase();
        }

        [Fact(Skip = "not ready yet.")]
        public async Task Explain_ExplainSingleQuery_SuccessfullyExplained()
        {
            // Arrange
            IMongoCollection<Bar> barCollection =
                _mongoDatabase.GetCollection<Bar>();

            FindOptions findOptions = new FindOptions
            {
                Collation = new Collation(
                    locale: "en",
                    strength: CollationStrength.Secondary)
            };

            FilterDefinition<Bar> filter =
                Builders<Bar>.Filter.And(
                    Builders<Bar>.Filter.Eq(u => u.Id, "1234"),
                    Builders<Bar>.Filter.Eq(b => b.Name, "NN"));

            // Act
            string? mongodbExplain = barCollection.Explain(filter, findOptions);            

            // Assert
            Snapshot.Match(mongodbExplain);
        }
    }
}
