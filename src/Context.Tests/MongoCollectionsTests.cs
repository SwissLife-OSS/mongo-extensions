using System;
using MongoDB.Driver;
using MongoDB.Extensions.Context.Internal;
using Moq;
using Xunit;

namespace MongoDB.Extensions.Context.Tests
{
    public class MongoCollectionsTests
    {
        #region Add Tests

        [Fact]
        public void Add_AddOneMongoCollection_SuccessfullyAdded()
        {
            // Arrange
            var mongoCollectionFooMock = new Mock<IMongoCollection<Foo>>();

            MongoCollections mongoCollections = new MongoCollections();

            // Act
            mongoCollections.Add(mongoCollectionFooMock.Object);

            // Assert
            Assert.True(mongoCollections.Exists<Foo>());
        }

        [Fact]
        public void Add_AddTwoMongoCollection_SuccessfullyAdded()
        {
            // Arrange
            var mongoCollectionFooMock = new Mock<IMongoCollection<Foo>>();
            var mongoCollectionBarMock = new Mock<IMongoCollection<Bar>>();

            MongoCollections mongoCollections = new MongoCollections();

            // Act
            mongoCollections.Add(mongoCollectionFooMock.Object);
            mongoCollections.Add(mongoCollectionBarMock.Object);

            // Assert
            Assert.True(mongoCollections.Exists<Foo>());
            Assert.True(mongoCollections.Exists<Bar>());
        }

        [Fact]
        public void Add_AddSameTypeSecondType_ThrowsException()
        {
            // Arrange
            var mongoCollectionFooMock = new Mock<IMongoCollection<Foo>>();

            MongoCollections mongoCollections = new MongoCollections();

            mongoCollections.Add(mongoCollectionFooMock.Object);

            // Act
            Action action = () => mongoCollections.Add(mongoCollectionFooMock.Object);

            // Assert
            Assert.Throws<ArgumentException>(action);
            Assert.True(mongoCollections.Exists<Foo>());
            Assert.Equal(1, mongoCollections.Count);
        }

        #endregion

        #region Exists Tests

        [Fact]
        public void Exists_MongoCollectionTypeExists_ReturnsTrue()
        {
            // Arrange
            var mongoCollectionFooMock = new Mock<IMongoCollection<Foo>>();

            MongoCollections mongoCollections = new MongoCollections();

            mongoCollections.Add(mongoCollectionFooMock.Object);

            // Act
            bool result = mongoCollections.Exists<Foo>();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Exists_MongoCollectionTypeNotExists_ReturnsFalse()
        {
            // Arrange
            var mongoCollectionFooMock = new Mock<IMongoCollection<Foo>>();

            MongoCollections mongoCollections = new MongoCollections();

            mongoCollections.Add(mongoCollectionFooMock.Object);

            // Act
            bool result = mongoCollections.Exists<Bar>();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Exists_NoCollectionsAdded_ReturnsFalse()
        {
            // Arrange
            MongoCollections mongoCollections = new MongoCollections();

            // Act
            bool result = mongoCollections.Exists<Bar>();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Count Tests

        [Fact]
        public void Count_OneMongoCollectionExists_ReturnsOne()
        {
            // Arrange
            var mongoCollectionFooMock = new Mock<IMongoCollection<Foo>>();

            MongoCollections mongoCollections = new MongoCollections();

            mongoCollections.Add(mongoCollectionFooMock.Object);

            // Act
            int count = mongoCollections.Count;

            // Assert
            Assert.Equal(1, count);
        }

        [Fact]
        public void Count_TwoMongoCollectionExists_ReturnsTwo()
        {
            // Arrange
            var mongoCollectionFooMock = new Mock<IMongoCollection<Foo>>();
            var mongoCollectionBarMock = new Mock<IMongoCollection<Bar>>();

            MongoCollections mongoCollections = new MongoCollections();

            mongoCollections.Add(mongoCollectionFooMock.Object);
            mongoCollections.Add(mongoCollectionBarMock.Object);

            // Act
            int count = mongoCollections.Count;

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public void Count_NoMongoCollectionExists_ReturnsZero()
        {
            // Arrange
            MongoCollections mongoCollections = new MongoCollections();

            // Act
            int count = mongoCollections.Count;

            // Assert
            Assert.Equal(0, count);
        }

        #endregion

        #region TryGetCollection Tests

        [Fact]
        public void TryGetCollection_FindAddedMongoCollectionByType_ReturnsRightMongoCollection()
        {
            // Arrange
            var mongoCollectionFooMock = new Mock<IMongoCollection<Foo>>();
            var mongoCollectionBarMock = new Mock<IMongoCollection<Bar>>();

            MongoCollections mongoCollections = new MongoCollections();

            mongoCollections.Add(mongoCollectionFooMock.Object);
            mongoCollections.Add(mongoCollectionBarMock.Object);

            // Act
            IMongoCollection<Bar>? mongoCollection =
                mongoCollections.TryGetCollection<Bar>();

            // Assert
            Assert.NotNull(mongoCollection);
            Assert.Same(mongoCollectionBarMock.Object, mongoCollection);
        }

        [Fact]
        public void TryGetCollection_FindNotExistingMongoCollectionByType_ReturnsNull()
        {
            // Arrange
            var mongoCollectionFooMock = new Mock<IMongoCollection<Foo>>();

            MongoCollections mongoCollections = new MongoCollections();

            mongoCollections.Add(mongoCollectionFooMock.Object);

            // Act
            IMongoCollection<Bar>? mongoCollection =
                mongoCollections.TryGetCollection<Bar>();

            // Assert
            Assert.Null(mongoCollection);
        }

#endregion
    }
}
