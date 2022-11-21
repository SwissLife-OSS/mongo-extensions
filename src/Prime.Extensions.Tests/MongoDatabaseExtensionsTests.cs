using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Snapshooter.Xunit;
using Squadron;
using Xunit;

namespace MongoDB.Prime.Extensions.Tests
{
    public class MongoDatabaseExtensionsTests : IClassFixture<MongoResource>
    {
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDatabaseExtensionsTests(MongoResource mongoResource)
        {
            _mongoDatabase = mongoResource.CreateDatabase();
        }

        #region EnableProfiling Tests

        [Fact]
        public void EnableProfiling_EnableDefaultProfileLevel_EnabledProfilingOnLevel2()
        {
            // Arrange

            // Act
            _mongoDatabase.EnableProfiling();

            // Assert
            var profileStatusCommand = new BsonDocument("profile", -1);
            BsonDocument result = _mongoDatabase.RunCommand<BsonDocument>(profileStatusCommand);

            Assert.Equal(
                "{ \"was\" : 2, \"slowms\" : 100, \"sampleRate\" : 1.0, \"ok\" : 1.0 }",
                result.ToString());
        }

        [Fact]
        public void EnableProfiling_EnableAllProfileLevel_EnabledProfilingOnLevel2()
        {
            // Arrange

            // Act
            _mongoDatabase.EnableProfiling(ProfileLevel.All);

            // Assert
            var profileStatusCommand = new BsonDocument("profile", -1);
            BsonDocument result = _mongoDatabase.RunCommand<BsonDocument>(profileStatusCommand);

            Assert.Equal(
                "{ \"was\" : 2, \"slowms\" : 100, \"sampleRate\" : 1.0, \"ok\" : 1.0 }",
                result.ToString());
        }

        [Fact]
        public void EnableProfiling_EnableSlowOperationsOnlyProfileLevel_EnabledProfilingOnLevel1()
        {
            // Arrange

            // Act
            _mongoDatabase.EnableProfiling(ProfileLevel.SlowOperationsOnly);

            // Assert
            var profileStatusCommand = new BsonDocument("profile", -1);
            BsonDocument result = _mongoDatabase.RunCommand<BsonDocument>(profileStatusCommand);

            Assert.Equal(
                "{ \"was\" : 1, \"slowms\" : 100, \"sampleRate\" : 1.0, \"ok\" : 1.0 }",
                result.ToString());
        }

        [Fact]
        public void EnableProfiling_DisableProfileLevel_EnabledProfilingOnLevel1()
        {
            // Arrange

            // Act
            _mongoDatabase.EnableProfiling(ProfileLevel.Off);

            // Assert
            var profileStatusCommand = new BsonDocument("profile", -1);
            BsonDocument result = _mongoDatabase.RunCommand<BsonDocument>(profileStatusCommand);

            Assert.Equal(
                "{ \"was\" : 0, \"slowms\" : 100, \"sampleRate\" : 1.0, \"ok\" : 1.0 }",
                result.ToString());
        }

        #endregion GetProfilingStatus Tests

        #region GetProfilingStatus Tests

        [Fact]
        public void GetProfilingStatus_GetDisabledProfileStatus_StatusDisabled()
        {
            // Arrange
            var profileCommand = new BsonDocument("profile", 0);
            _mongoDatabase.RunCommand<BsonDocument>(profileCommand);

            // Act
            ProfilingStatus profileStatus = _mongoDatabase.GetProfilingStatus();

            // Assert
            Assert.Equal(ProfileLevel.Off, profileStatus.Level);
            Assert.Equal(100, profileStatus.SlowMs);
            Assert.Equal(1.0d, profileStatus.SampleRate);
            Assert.Equal("1", profileStatus.Filter);
        }

        [Fact]
        public void GetProfilingStatus_GetEnabledProfileStatusSlow_StatusSlow()
        {
            // Arrange
            var profileCommand = new BsonDocument("profile", 1);
            _mongoDatabase.RunCommand<BsonDocument>(profileCommand);

            // Act
            ProfilingStatus profileStatus = _mongoDatabase.GetProfilingStatus();

            // Assert
            Assert.Equal(ProfileLevel.SlowOperationsOnly, profileStatus.Level);
            Assert.Equal(100, profileStatus.SlowMs);
            Assert.Equal(1.0d, profileStatus.SampleRate);
            Assert.Equal("1", profileStatus.Filter);
        }

        [Fact]
        public void GetProfilingStatus_GetEnabledProfileStatusAll_StatusAll()
        {
            // Arrange
            var profileCommand = new BsonDocument("profile", 2);
            _mongoDatabase.RunCommand<BsonDocument>(profileCommand);

            // Act
            ProfilingStatus profileStatus = _mongoDatabase.GetProfilingStatus();

            // Assert
            Assert.Equal(ProfileLevel.All, profileStatus.Level);
            Assert.Equal(100, profileStatus.SlowMs);
            Assert.Equal(1.0d, profileStatus.SampleRate);
            Assert.Equal("1", profileStatus.Filter);
        }

        #endregion GetProfilingStatus Tests

        #region GetProfiledOperations Tests

        [Fact]
        public void GetProfiledOperations_GetOneExecutedOperations_ReturnsOneMongoDBOperation()
        {
            // Arrange
            _mongoDatabase.EnableProfiling();

            _mongoDatabase.CreateCollection("Bar");

            // Act
            IEnumerable<string> results =
                _mongoDatabase.GetProfiledOperations();

            // Assert
            Snapshot.Match(results.Single(),
                matchOptions => matchOptions
                    .IncludeField("**.command")                    
                    .IncludeField("**.keysExamined")                    
                    .IncludeField("**.docsExamined")                    
                    .IncludeField("**.planSummary")                    
                    .IncludeField("**.execStats")
                    .ExcludeField("**.$db")
                    .ExcludeField("**.lsid")
                );
        }

        [Fact]
        public void GetProfiledOperations_GetAllExecutedOperations_ReturnsAllMongoDBOperations()
        {
            // Arrange
            _mongoDatabase.EnableProfiling();

            _mongoDatabase.CreateCollection("Bar");
            _mongoDatabase.CreateCollection("Foo");
            _mongoDatabase.GetCollection<Bar>()
                .InsertOne(new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Bar1", "Value1"));
            _mongoDatabase.GetCollection<Foo>().InsertOne(new Foo("Foo1", "Value1"));
            _mongoDatabase.GetCollection<Foo>().Find(foo => foo.Name == "foo1").ToList();

            // Act
            string[] results =
                _mongoDatabase.GetProfiledOperations().ToArray();

            // Assert
            Snapshot.Match(results.ToJsonArray(),
                matchOptions => matchOptions
                    .IncludeField("**.command")
                    .IncludeField("**.keysExamined")
                    .IncludeField("**.docsExamined")
                    .IncludeField("**.planSummary")
                    .IncludeField("**.execStats")
                    .ExcludeField("**.$db")
                    .ExcludeField("**.lsid")
                );
        }

        #endregion

        #region CleanAllCollections Tests

        [Fact]
        public async Task CleanAllCollections_CleanAllDocumentsOfAllCollections_AllCollectionsEmpty()
        {
            // Arrange
            IMongoCollection<Bar> barCollection = _mongoDatabase.GetCollection<Bar>();
            IMongoCollection<Foo> fooCollection = _mongoDatabase.GetCollection<Foo>();

            var arrangedBars = new List<Bar> {
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Bar1", "Value1"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Bar2", "Value2"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Bar3", "Value3"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Bar3", "Value4"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903045"), "Bar5", "Value5"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"), "Bar6", "Value6"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903047"), "Bar7", "Value7"),
            };

            var arrangedFoo = new List<Foo> {
               new Foo("Foo1", "Value1"),
               new Foo("Foo2", "Value2"),
               new Foo("Foo3", "Value3"),
               new Foo("Foo4", "Value4"),
               new Foo("Foo5", "Value5")
            };

            await barCollection.InsertManyAsync(arrangedBars);
            await fooCollection.InsertManyAsync(arrangedFoo);

            Assert.Equal(7, barCollection.CountDocuments());
            Assert.Equal(5, fooCollection.CountDocuments());

            // Act
            _mongoDatabase.CleanAllCollections();

            // Assert
            Assert.Equal(0, barCollection.CountDocuments());
            Assert.Equal(0, fooCollection.CountDocuments());
        }

        [Fact]
        public async Task CleanAllCollectionsAsync_CleanAllDocumentsOfAllCollections_AllCollectionsEmpty()
        {
            // Arrange
            IMongoCollection<Bar> barCollection = _mongoDatabase.GetCollection<Bar>();
            IMongoCollection<Foo> fooCollection = _mongoDatabase.GetCollection<Foo>();

            var arrangedBars = new List<Bar> {
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903041"), "Bar1", "Value1"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903042"), "Bar2", "Value2"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903043"), "Bar3", "Value3"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903044"), "Bar3", "Value4"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903045"), "Bar5", "Value5"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903046"), "Bar6", "Value6"),
               new Bar(Guid.Parse("A1C9E3E8-B448-42DA-A684-716932903047"), "Bar7", "Value7"),
            };

            var arrangedFoo = new List<Foo> {
               new Foo("Foo1", "Value1"),
               new Foo("Foo2", "Value2"),
               new Foo("Foo3", "Value3"),
               new Foo("Foo4", "Value4"),
               new Foo("Foo5", "Value5")
            };

            await barCollection.InsertManyAsync(arrangedBars);
            await fooCollection.InsertManyAsync(arrangedFoo);

            Assert.Equal(7, barCollection.CountDocuments());
            Assert.Equal(5, fooCollection.CountDocuments());

            // Act
            await _mongoDatabase.CleanAllCollectionsAsync();

            // Assert
            Assert.Equal(0, barCollection.CountDocuments());
            Assert.Equal(0, fooCollection.CountDocuments());
        }

        #endregion CleanAllCollections Tests        
    }
}
