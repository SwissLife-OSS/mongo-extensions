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

        [Fact]
        public void CleanCollection_CleanAllDocumentsOfACollection_Cleaned()
        {
            // Arrange
            IMongoCollection<Bar> barCollection =
                _mongoDatabase.GetCollection<Bar>();

            barCollection.InsertMany(new[]
            {
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Bar1", "Value1"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Bar2", "Value2"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Bar3", "Value3"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Bar4", "Value4"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903045"), "Bar5", "Value5"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"), "Bar6", "Value6"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903047"), "Bar7", "Value7"),
            });

            Assert.Equal(7, _mongoDatabase.GetCollection<Bar>().CountDocuments());

            // Act
            barCollection.CleanCollection<Bar>();

            // Assert
            Assert.Equal(0, _mongoDatabase.GetCollection<Bar>().CountDocuments());
        }

        [Fact]
        public async Task CleanCollectionAsync_CleanAllDocumentsOfACollection_Cleaned()
        {
            // Arrange
            IMongoCollection<Bar> barCollection =
                _mongoDatabase.GetCollection<Bar>();

            barCollection.InsertMany(new[]
            {
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Bar1", "Value1"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Bar2", "Value2"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Bar3", "Value3"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Bar4", "Value4"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903045"), "Bar5", "Value5"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"), "Bar6", "Value6"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903047"), "Bar7", "Value7"),
            });

            Assert.Equal(7, _mongoDatabase.GetCollection<Bar>().CountDocuments());

            // Act
            await barCollection.CleanCollectionAsync<Bar>();

            // Assert
            Assert.Equal(0, _mongoDatabase.GetCollection<Bar>().CountDocuments());
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
                    Builders<Bar>.Filter.Eq(u => u.Id, Guid.NewGuid()),
                    Builders<Bar>.Filter.Eq(b => b.Name, "NN"));

            // Act
            string? mongodbExplain = barCollection.Explain(filter, findOptions);            

            // Assert
            Snapshot.Match(mongodbExplain);
        }
    }
}
