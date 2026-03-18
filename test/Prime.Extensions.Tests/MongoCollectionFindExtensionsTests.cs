using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Snapshooter.Xunit;
using Squadron;
using Xunit;
#nullable enable

namespace MongoDB.Prime.Extensions.Tests
{
    public class MongoCollectionFindExtensionsTests : IClassFixture<MongoResource>
    {
        private readonly IMongoDatabase _mongoDatabase;

        public MongoCollectionFindExtensionsTests(MongoResource mongoResource)
        {
            _mongoDatabase = mongoResource.CreateDatabase();
        }

        #region FindIds Tests

        [Fact]
        public async Task FindIds_FindOneId_ReturnsRightBar()
        {
            // Arrange
            IMongoCollection<Bar> barCollection = _mongoDatabase.GetCollection<Bar>();

            var arrangedBar = new Bar(
                Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Bar1", "Value1");

            await barCollection.InsertOneAsync(arrangedBar);

            var idsToFind = new List<Guid> { arrangedBar.Id };

            // Act
            IReadOnlyDictionary<Guid, Bar> result = 
                await barCollection.FindUniqueIdsAsync(idsToFind, bar => bar.Id);

            // Assert
            Snapshot.Match(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task FindIds_FindFourBarIdsSynchronously_ReturnsRightBars(
            int? parallelBatchSize)
        {
            // Arrange
            IMongoCollection<Bar> barCollection = _mongoDatabase.GetCollection<Bar>();

            var arrangedBars = new List<Bar> {
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Bar1", "Value1"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Bar2", "Value2"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Bar3", "Value3"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Bar4", "Value4"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903045"), "Bar5", "Value5"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"), "Bar6", "Value6"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903047"), "Bar7", "Value7"),
            };

            await barCollection.InsertManyAsync(arrangedBars);

            IEnumerable<Guid> idsToFind = arrangedBars
                .GetRange(2, 4).Select(bar => bar.Id);

            // Act
            IReadOnlyDictionary<Guid, Bar> result = await barCollection
                .FindUniqueIdsAsync(idsToFind, bar => bar.Id, parallelBatchSize: parallelBatchSize);

            // Assert
            Assert.IsType<Dictionary<Guid, Bar>>(result);
            Snapshot.Match(result.OrderBy(entry => entry.Key));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task FindIds_FindFourBarIdsAsynchronously_ReturnsRightBars(
            int parallelBatchSize)
        {
            // Arrange
            IMongoCollection<Bar> barCollection = _mongoDatabase.GetCollection<Bar>();

            var arrangedBars = new List<Bar> {
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Bar1", "Value1"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Bar2", "Value2"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Bar3", "Value3"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Bar4", "Value4"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903045"), "Bar5", "Value5"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"), "Bar6", "Value6"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903047"), "Bar7", "Value7"),
            };

            await barCollection.InsertManyAsync(arrangedBars);

            IEnumerable<Guid> idsToFind = arrangedBars
                .GetRange(2, 4).Select(bar => bar.Id);

            // Act
            IReadOnlyDictionary<Guid, Bar> result = await barCollection
                .FindUniqueIdsAsync(idsToFind, bar => bar.Id, parallelBatchSize: parallelBatchSize);

            // Assert
            Assert.IsType<ConcurrentDictionary<Guid, Bar>>(result);
            Snapshot.Match(result.OrderBy(key => key.Key));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task FindIds_FindFourBarNamesSynchronously_ReturnsRightBars(
            int? parallelBatchSize)
        {
            // Arrange
            IMongoCollection<Bar> barCollection = _mongoDatabase.GetCollection<Bar>();

            var arrangedBars = new List<Bar> {
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Bar1", "Value1"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Bar2", "Value2"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Bar3", "Value3"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Bar4", "Value4"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903045"), "Bar5", "Value5"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"), "Bar6", "Value6"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903047"), "Bar7", "Value7"),
            };

            await barCollection.InsertManyAsync(arrangedBars);

            IEnumerable<string> namesToFind = arrangedBars
                .GetRange(2, 4).Select(bar => bar.Name);

            // Act
            IReadOnlyDictionary<string, Bar> result = await barCollection
                .FindUniqueIdsAsync(namesToFind, bar => bar.Name, parallelBatchSize: parallelBatchSize);

            // Assert
            Assert.IsType<Dictionary<string, Bar>>(result);
            Snapshot.Match(result.OrderBy(entry => entry.Key));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task FindIds_FindFourBarNamesAsynchronously_ReturnsRightBars(
            int parallelBatchSize)
        {
            // Arrange
            IMongoCollection<Bar> barCollection = _mongoDatabase.GetCollection<Bar>();

            var arrangedBars = new List<Bar> {
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Bar1", "Value1"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Bar2", "Value2"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Bar3", "Value3"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Bar4", "Value4"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903045"), "Bar5", "Value5"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"), "Bar6", "Value6"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903047"), "Bar7", "Value7"),
            };

            await barCollection.InsertManyAsync(arrangedBars);

            IEnumerable<string> namesToFind = arrangedBars
                .GetRange(2, 4).Select(bar => bar.Name);

            // Act
            IReadOnlyDictionary<string, Bar> result = await barCollection
                .FindUniqueIdsAsync(namesToFind, bar => bar.Name, parallelBatchSize: parallelBatchSize);

            // Assert
            Assert.IsType<ConcurrentDictionary<string, Bar>>(result);
            Snapshot.Match(result.OrderBy(key => key.Key));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task FindIds_FindDuplicatedBarIdsSynchronously_ReturnsDistinctBars(
            int? parallelBatchSize)
        {
            // Arrange
            IMongoCollection<Bar> barCollection = _mongoDatabase.GetCollection<Bar>();

            var arrangedBars = new List<Bar> {
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Bar1", "Value1"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Bar2", "Value2"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Bar3", "Value3"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Bar4", "Value4"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903045"), "Bar5", "Value5"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"), "Bar6", "Value6"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903047"), "Bar7", "Value7"),
            };

            await barCollection.InsertManyAsync(arrangedBars);

            IEnumerable<Guid> duplicatedIds = new List<Guid> {
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"),
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"),
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"),
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"),
            };

            // Act
            IReadOnlyDictionary<Guid, Bar> result = await barCollection
                .FindUniqueIdsAsync(duplicatedIds, bar => bar.Id, parallelBatchSize: parallelBatchSize);

            // Assert
            Assert.IsType<Dictionary<Guid, Bar>>(result);
            Snapshot.Match(result.OrderBy(entry => entry.Key));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task FindIds_FindDuplicatedBarIdsAsynchronously_ReturnsDistinctBars(
            int parallelBatchSize)
        {
            // Arrange
            IMongoCollection<Bar> barCollection = _mongoDatabase.GetCollection<Bar>();

            var arrangedBars = new List<Bar> {
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Bar1", "Value1"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Bar2", "Value2"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Bar3", "Value3"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Bar4", "Value4"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903045"), "Bar5", "Value5"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"), "Bar6", "Value6"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903047"), "Bar7", "Value7"),
            };

            await barCollection.InsertManyAsync(arrangedBars);

            IEnumerable<Guid> duplicatedIds = new List<Guid> {
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"),
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"),
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"),
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"),
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903047")
            };

            // Act
            IReadOnlyDictionary<Guid, Bar> result = await barCollection
                .FindUniqueIdsAsync(duplicatedIds, bar => bar.Id, parallelBatchSize: parallelBatchSize);

            // Assert
            Assert.IsType<ConcurrentDictionary<Guid, Bar>>(result);
            Snapshot.Match(result.OrderBy(key => key.Key));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task FindIds_FindDuplicatedBarNamesSynchronously_ThrowsArgumentException(
            int? parallelBatchSize)
        {
            // Arrange
            IMongoCollection<Bar> barCollection = _mongoDatabase.GetCollection<Bar>();

            var arrangedBars = new List<Bar> {
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Bar1", "Value1"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Bar2", "Value2"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Bar3", "Value3"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Bar3", "Value4"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903045"), "Bar5", "Value5"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"), "Bar6", "Value6"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903047"), "Bar7", "Value7"),
            };

            await barCollection.InsertManyAsync(arrangedBars);

            IEnumerable<string> namesToFind = arrangedBars
                .GetRange(2, 4).Select(bar => bar.Name);

            // Act
            Func<Task> action = async () => await barCollection
                .FindUniqueIdsAsync(namesToFind, bar => bar.Name, parallelBatchSize: parallelBatchSize);

            // Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(action);
            Assert.Contains("Bar3", exception.Message);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task FindIds_FindDuplicatedBarNamesAsynchronously_ThrowsArgumentException(
            int parallelBatchSize)
        {
            // Arrange
            IMongoCollection<Bar> barCollection = _mongoDatabase.GetCollection<Bar>();

            var arrangedBars = new List<Bar> {
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Bar1", "Value1"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Bar2", "Value2"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Bar3", "Value3"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Bar4", "Value4"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903045"), "Bar4", "Value5"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"), "Bar6", "Value6"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903047"), "Bar7", "Value7"),
            };

            await barCollection.InsertManyAsync(arrangedBars);

            IEnumerable<string> namesToFind = arrangedBars
                .GetRange(2, 4).Select(bar => bar.Name);

            // Act
            Func<Task> action = async () => await barCollection
                .FindUniqueIdsAsync(namesToFind, bar => bar.Name, parallelBatchSize: parallelBatchSize);

            // Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(action);
            Assert.Contains("Bar4", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task FindIds_FindNotExistingBarIdsSynchronously_ReturnsEmptyDictionary(
            int? parallelBatchSize)
        {
            // Arrange
            IMongoCollection<Bar> barCollection = _mongoDatabase.GetCollection<Bar>();

            var arrangedBars = new List<Bar> {
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Bar1", "Value1"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Bar2", "Value2"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Bar3", "Value3"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Bar4", "Value4"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903045"), "Bar5", "Value5"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"), "Bar6", "Value6"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903047"), "Bar7", "Value7"),
            };

            await barCollection.InsertManyAsync(arrangedBars);

            IEnumerable<Guid> notExistingIds = new List<Guid> {
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903048"),
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903049"),
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903010"),
            };

            // Act
            IReadOnlyDictionary<Guid, Bar> result = await barCollection
                .FindUniqueIdsAsync(notExistingIds, bar => bar.Id, parallelBatchSize: parallelBatchSize);

            // Assert
            Assert.IsType<Dictionary<Guid, Bar>>(result);
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task FindIds_FindNotExistingIdsAsynchronously_ReturnsEmptyDictionary(
            int parallelBatchSize)
        {
            // Arrange
            IMongoCollection<Bar> barCollection = _mongoDatabase.GetCollection<Bar>();

            var arrangedBars = new List<Bar> {
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Bar1", "Value1"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Bar2", "Value2"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Bar3", "Value3"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Bar4", "Value4"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903045"), "Bar5", "Value5"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"), "Bar6", "Value6"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903047"), "Bar7", "Value7"),
            };

            await barCollection.InsertManyAsync(arrangedBars);

            IEnumerable<Guid> notExistingIds = new List<Guid> {
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903048"),
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903049"),
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903010"),
               Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903011"),
            };

            // Act
            IReadOnlyDictionary<Guid, Bar> result = await barCollection
                .FindUniqueIdsAsync(notExistingIds, bar => bar.Id, parallelBatchSize: parallelBatchSize);

            // Assert
            Assert.IsType<ConcurrentDictionary<Guid, Bar>>(result);
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(249, typeof(Dictionary<Guid, Bar>), null)]
        [InlineData(249, typeof(ConcurrentDictionary<Guid, Bar>), 5)]
        [InlineData(250, typeof(ConcurrentDictionary<Guid, Bar>), null)]
        [InlineData(250, typeof(Dictionary<Guid, Bar>), 500)]
        [InlineData(251, typeof(ConcurrentDictionary<Guid, Bar>), null)]
        [InlineData(251, typeof(Dictionary<Guid, Bar>), 251)]
        [InlineData(500, typeof(ConcurrentDictionary<Guid, Bar>), null)]
        [InlineData(500, typeof(Dictionary<Guid, Bar>), 500)]
        public async Task FindIds_FindBarsByIds_ReturnsRightBars(
            int numberOfBarsToFind, Type dictionaryType, int? parallelBatchSize)
        {
            // Arrange
            IMongoCollection<Bar> barCollection = _mongoDatabase.GetCollection<Bar>();

            IEnumerable<Bar> alldBars = CreateRandomBars(1000);

            await barCollection.InsertManyAsync(alldBars);

            IEnumerable<Bar> barsToFind = alldBars.Skip(200).Take(numberOfBarsToFind);

            IEnumerable<Guid> barIdsToFind = barsToFind.Select(d => d.Id);

            // Act
            IReadOnlyDictionary<Guid, Bar> result = await barCollection
                .FindUniqueIdsAsync(barIdsToFind, bar => bar.Id, parallelBatchSize: parallelBatchSize);

            // Assert
            Assert.IsType(dictionaryType, result);
            Assert.Equal<string>(
                barIdsToFind.Select(x => x.ToString()).OrderBy(q => q).ToList(),
                result.Keys.Select(x => x.ToString()).OrderBy(q => q).ToList());
            Assert.Equal<string>(
                barIdsToFind.Select(x => x.ToString()).OrderBy(q => q).ToList(),
                result.Values.Select(v => v.Id.ToString()).OrderBy(q => q).ToList());
        }

        [Theory]
        [InlineData(10000, typeof(Dictionary<Guid, Bar>), 10000)]
        [InlineData(10000, typeof(ConcurrentDictionary<Guid, Bar>), null)]
        public async Task FindIds_FindLargeAmountOfBarsByIds_ReturnsRightBars(
            int numberOfBarsToFind, Type dictionaryType, int? parallelBatchSize)
        {
            // Arrange
            IMongoCollection<Bar> barCollection = _mongoDatabase.GetCollection<Bar>();

            IEnumerable<Bar> alldBars = CreateRandomBars(20000);

            await barCollection.InsertManyAsync(alldBars);

            IEnumerable<Bar> barsToFind = alldBars.Skip(500).Take(numberOfBarsToFind);

            IEnumerable<Guid> barIdsToFind = barsToFind.Select(d => d.Id);

            // Act
            IReadOnlyDictionary<Guid, Bar> result = await barCollection
                .FindUniqueIdsAsync(barIdsToFind, bar => bar.Id, parallelBatchSize: parallelBatchSize);

            // Assert
            Assert.IsType(dictionaryType, result);
            Assert.Equal<string>(
                barIdsToFind.Select(x => x.ToString()).OrderBy(q => q).ToList(),
                result.Keys.Select(x => x.ToString()).OrderBy(q => q).ToList());
            Assert.Equal<string>(
                barIdsToFind.Select(x => x.ToString()).OrderBy(q => q).ToList(),
                result.Values.Select(v => v.Id.ToString()).OrderBy(q => q).ToList());
        }

        #endregion

        #region CalculateRequestPartitionCount Tests

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(0, 5, 0)]
        [InlineData(1, 0, 0)]
        [InlineData(1, 1, 1)]
        [InlineData(1, 2, 0)]
        [InlineData(5, -2, 0)]
        [InlineData(5, 0, 0)]
        [InlineData(20, 1, 20)]
        [InlineData(20, 2, 10)]
        [InlineData(20, 3, 6)]
        [InlineData(20, 4, 5)]
        [InlineData(20, 5, 4)]
        [InlineData(20, 6, 3)]
        [InlineData(20, 7, 2)]
        [InlineData(20, 8, 2)]
        [InlineData(20, 9, 2)]
        [InlineData(20, 10, 2)]
        [InlineData(20, 11, 1)]
        [InlineData(20, 19, 1)]
        [InlineData(20, 20, 1)]
        [InlineData(20, 21, 0)]
        [InlineData(200, 20, 10)]
        [InlineData(200, 15, 13)]
        [InlineData(200, 1, 200)]
        [InlineData(200, 0, 0)]
        [InlineData(200, -1, 0)]
        [InlineData(200, -5, 0)]
        [InlineData(200, 200, 1)]
        public void CalculateRequestPartitionCount_CalculateCountWithBatchSize_ReturnedRightPartitionCount(
            int totalIdsCount, int batchSize, int result)
        {
            // Arrange

            // Act
            int partitionCount = MongoCollectionFindExtensions
                .CalculateRequestPartitionCount(totalIdsCount, batchSize);

            // Assert
            Assert.Equal(result, partitionCount);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(100, 1)]
        [InlineData(200, 1)]
        [InlineData(250, 2)]
        [InlineData(251, 2)]
        [InlineData(252, 2)]
        [InlineData(374, 2)]
        [InlineData(375, 3)]
        [InlineData(376, 3)]
        [InlineData(499, 3)]
        [InlineData(500, 2)]
        [InlineData(750, 3)]
        [InlineData(999, 3)]
        [InlineData(1000, 2)]
        [InlineData(1001, 2)]
        [InlineData(1499, 2)]
        [InlineData(1500, 3)]
        [InlineData(1999, 3)]
        [InlineData(2000, 4)]
        [InlineData(3000, 6)]
        [InlineData(4000, 8)]
        [InlineData(4999, 9)]
        [InlineData(5000, 5)]
        [InlineData(6000, 6)]
        [InlineData(7000, 7)]
        [InlineData(8000, 8)]
        [InlineData(9000, 9)]
        [InlineData(9999, 9)]
        [InlineData(10000, 5)]
        [InlineData(11000, 5)]
        [InlineData(12000, 6)]
        [InlineData(13000, 6)]
        [InlineData(14000, 7)]
        [InlineData(15000, 7)]
        [InlineData(16000, 8)]
        [InlineData(17000, 8)]
        [InlineData(18000, 9)]
        [InlineData(19000, 9)]
        [InlineData(20000, 4)]
        [InlineData(30000, 6)]
        [InlineData(40000, 8)]
        [InlineData(49999, 9)]
        [InlineData(50000, 5)]
        [InlineData(60000, 6)]
        [InlineData(70000, 7)]
        [InlineData(80000, 8)]
        [InlineData(90000, 9)]
        [InlineData(100000, 5)]
        [InlineData(110000, 5)]
        [InlineData(120000, 6)]
        [InlineData(130000, 6)]
        [InlineData(140000, 7)]
        [InlineData(150000, 7)]
        [InlineData(160000, 8)]
        [InlineData(170000, 8)]
        [InlineData(180000, 9)]
        [InlineData(190000, 9)]
        [InlineData(200000, 4)]
        [InlineData(300000, 6)]
        [InlineData(400000, 8)]
        [InlineData(500000, 5)]
        [InlineData(600000, 6)]
        [InlineData(700000, 7)]
        [InlineData(800000, 8)]
        [InlineData(900000, 9)]
        [InlineData(1000000, 10)]
        [InlineData(2000000, 10)]
        public void CalculateRequestPartitionCount_CalculateCountWithDefaultBatchSize_ReturnedRightPartitionCount(
            int totalIdsCount, int result)
        {
            // Arrange

            // Act
            int partitionCount = MongoCollectionFindExtensions
                .CalculateRequestPartitionCount(totalIdsCount);

            // Assert
            Assert.Equal(result, partitionCount);
        }

        #endregion

        #region Private Helpers

        private IEnumerable<Bar> CreateRandomBars(int count)
        {
            return Enumerable
                .Range(0, count)
                .Select(number => new Bar(
                    Guid.NewGuid(),
                    $"BarName-Unique-{number}",
                    $"BarValue-{Guid.NewGuid()}"))
                .ToList();
        }
        
        #endregion
    }
}
