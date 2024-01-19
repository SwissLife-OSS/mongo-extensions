using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Snapshooter.Xunit;
using Squadron;
using Xunit;

namespace MongoDB.Prime.Extensions.Tests
{
    public class AsyncCursorSourceExtensionsTests : IClassFixture<MongoResource>
    {
        private readonly IMongoDatabase _mongoDatabase;

        public AsyncCursorSourceExtensionsTests(MongoResource mongoResource)
        {
            _mongoDatabase = mongoResource.CreateDatabase();
        }

        #region ToDictionaryAsync Tests

        [Fact]
        public async Task ToDictionaryAsync_FindOneDocument_ReturnRightDocument()
        {
            // Arrange
            IMongoCollection<Foo> fooCollection = _mongoDatabase.GetCollection<Foo>();

            var arrangedFoo = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Foo1");

            await fooCollection.InsertOneAsync(arrangedFoo);

            FilterDefinition<Foo> filter =
                Builders<Foo>.Filter.Eq(foo => foo.Id, arrangedFoo.Id);

            // Act
            Dictionary<Guid, Foo> documentBases = await fooCollection.Find(filter)
                .ToDictionaryAsync(foo => foo.Id, CancellationToken.None);

            // Assert
            KeyValuePair<Guid, Foo> resultFoo = documentBases.Single();
            Assert.NotSame(arrangedFoo, resultFoo.Value);
            Assert.Equal(arrangedFoo.Id, resultFoo.Key);
            Assert.Equal(arrangedFoo.Id, resultFoo.Value.Id);
            Assert.Equal(arrangedFoo.Name, resultFoo.Value.Name);
        }

        [Fact]
        public async Task ToDictionaryAsync_FindMultipleDocumentsWithNameAsKey_ReturnRightDocuments()
        {
            // Arrange
            IMongoCollection<Foo> fooCollection = _mongoDatabase.GetCollection<Foo>();

            var arrangedFoo1 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Foo1");
            var arrangedFoo2 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Foo2");
            var arrangedFoo3 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Foo3");
            var arrangedFoo4 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Foo4");

            await fooCollection.InsertManyAsync(
                new []{ arrangedFoo1, arrangedFoo2 , arrangedFoo3 , arrangedFoo4 });

            FilterDefinition<Foo> filter = Builders<Foo>.Filter.Empty;

            // Act
            Dictionary<string, Foo> documentBases = await fooCollection.Find(filter)
                .ToDictionaryAsync(foo => foo.Name, CancellationToken.None);

            // Assert
            Snapshot.Match(documentBases);
        }

        [Fact]
        public async Task ToDictionaryAsync_FindMultipleDocumentsWithIdAsKey_ReturnRightDocuments()
        {
            // Arrange
            IMongoCollection<Foo> fooCollection = _mongoDatabase.GetCollection<Foo>();

            var arrangedFoo1 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Foo1");
            var arrangedFoo2 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Foo2");
            var arrangedFoo3 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Foo3");
            var arrangedFoo4 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Foo4");

            await fooCollection.InsertManyAsync(
                new[] { arrangedFoo1, arrangedFoo2, arrangedFoo3, arrangedFoo4 });

            FilterDefinition<Foo> filter = Builders<Foo>.Filter.Empty;

            // Act
            Dictionary<Guid, Foo> documentBases = await fooCollection.Find(filter)
                .ToDictionaryAsync(foo => foo.Id, CancellationToken.None);

            // Assert
            Snapshot.Match(documentBases);
        }

        [Fact]
        public async Task ToDictionaryAsync_FindNoDocuments_ReturnEmptyDictionary()
        {
            // Arrange
            IMongoCollection<Foo> fooCollection = _mongoDatabase.GetCollection<Foo>();

            var arrangedFoo1 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Foo1");
            var arrangedFoo2 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Foo2");
            var arrangedFoo3 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Foo3");
            var arrangedFoo4 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Foo4");

            await fooCollection.InsertManyAsync(
                new[] { arrangedFoo1, arrangedFoo2, arrangedFoo3, arrangedFoo4 });

            FilterDefinition<Foo> filter =
                Builders<Foo>.Filter.Eq(foo => foo.Id, Guid.NewGuid());

            // Act
            Dictionary<string, Foo> documentBases = await fooCollection.Find(filter)
                .ToDictionaryAsync(foo => foo.Name, CancellationToken.None);

            // Assert
            Assert.Empty(documentBases);
        }

        [Fact]
        public async Task ToDictionaryAsync_DuplicateDocuments_ThrowsArgumentException()
        {
            // Arrange
            IMongoCollection<Foo> fooCollection = _mongoDatabase.GetCollection<Foo>();

            var arrangedFoo1 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Foo1");
            var arrangedFoo2 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Foo2");
            var arrangedFoo3 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Foo3");
            var arrangedFoo4 = new Foo(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Foo3");

            await fooCollection.InsertManyAsync(
                new[] { arrangedFoo1, arrangedFoo2, arrangedFoo3, arrangedFoo4 });

            FilterDefinition<Foo> filter = Builders<Foo>.Filter.Empty;

            // Act
            Func<Task> action = async () => await fooCollection.Find(filter)
                .ToDictionaryAsync(foo => foo.Name, CancellationToken.None);

            // Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(action);
            Assert.Contains("Foo3", exception.Message);
        }

        [Fact]
        public async Task ToDictionaryAsync_FindHugeAmountOfDocuments_ReturnRightDocuments()
        {
            // Arrange
            IMongoCollection<Foo> fooCollection = _mongoDatabase.GetCollection<Foo>();

            var arrangedFoos = CreateRandomFooBatch(20000).ToList();

            await fooCollection.InsertManyAsync(arrangedFoos);

            var fooIdsToFind = arrangedFoos.Skip(5000).Take(10000).Select(d => d.Id).ToList();
            
            FilterDefinition<Foo> filter =
                Builders<Foo>.Filter.In(foo => foo.Id, fooIdsToFind);

            // Act
            Dictionary<Guid, Foo> documentBases = await fooCollection.Find(filter)
                .ToDictionaryAsync(foo => foo.Id, CancellationToken.None);

            // Assert
            Assert.Equal<string>(
                fooIdsToFind.Select(x => x.ToString()).OrderBy(q => q).ToList(),
                documentBases.Keys.Select(x => x.ToString()).OrderBy(q => q).ToList());
        }

        #endregion

        #region Private Helpers

        private IEnumerable<Foo> CreateRandomFooBatch(int batchSize)
        {
            return Enumerable.Range(0, batchSize)
                .Select(number => new Foo(Guid.NewGuid(), $"FooName{number}"));
        }

        private class Foo
        {
            public Foo(Guid id, string name)
            {
                Id = id;
                Name = name;
            }

            public Guid Id { get; }

            public string Name { get; }
        }

        #endregion
    }
}