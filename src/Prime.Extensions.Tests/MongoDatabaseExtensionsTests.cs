using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
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
                    .IgnoreField("**.ns")
                    .IgnoreField("**.$db")
                    .IgnoreField("**.flowControl")
                    .IgnoreField("**.millis")
                    .IgnoreField("**.ts")
                    .IgnoreField("**.base64")
                    .IgnoreField("**.client")
                    .IgnoreField("**.r")
                    .IgnoreField("**.ReplicationStateTransition")
                    .IgnoreField("**.FeatureCompatibilityVersion")
                );
        }

        [Fact]
        public void GetProfiledOperations_GetAllExecutedOperations_ReturnsAllMongoDBOperations()
        {
            // Arrange
            _mongoDatabase.EnableProfiling();

            _mongoDatabase.CreateCollection("Bar");
            _mongoDatabase.CreateCollection("Foo");
            _mongoDatabase.GetCollection<Bar>().InsertOne(new Bar("bar1"));
            _mongoDatabase.GetCollection<Foo>().InsertOne(new Foo("foo1"));
            _mongoDatabase.GetCollection<Foo>().Find(foo => foo.Name == "foo1").ToList();

            // Act
            string[] results =
                _mongoDatabase.GetProfiledOperations().ToArray();

            // Assert
            Snapshot.Match(results.ToJsonArray(),
                matchOptions => matchOptions
                    .IgnoreField("**.ns")
                    .IgnoreField("**.$db")
                    .IgnoreField("**.flowControl")
                    .IgnoreField("**.millis")
                    .IgnoreField("**.ts")
                    .IgnoreField("**.base64")
                    .IgnoreField("**.client")
                    .IgnoreField("**.r")
                    .IgnoreField("**.ReplicationStateTransition")
                    .IgnoreField("**.FeatureCompatibilityVersion")
                );
        }

        #endregion
    }
}
