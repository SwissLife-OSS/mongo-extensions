using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Extensions.Context.InterferingTests.Helpers;
using Snapshooter.Xunit;
using Squadron;
using Xunit;
using Xunit.Priority;

namespace MongoDB.Extensions.Context.Tests
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class MongoDatabaseBuilderTests : IClassFixture<MongoResource>
    {
        private readonly MongoOptions _mongoOptions;
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDatabaseBuilderTests(MongoResource mongoResource)
        {
            _mongoDatabase = mongoResource.CreateDatabase();
            _mongoOptions = new MongoOptions
            {
                ConnectionString = mongoResource.ConnectionString,
                DatabaseName = _mongoDatabase.DatabaseNamespace.DatabaseName
            };
        }

        #region ConfigureConnection Tests

        [Fact]
        public void ConfigureConnection_SetSpecificConnectionSettings_MongoConnectionSettingsSetSuccessfully()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

            // Act
            mongoDatabaseBuilder
                .ConfigureConnection(settings => settings.ApplicationName = "Test")
                .ConfigureConnection(settings => settings.DirectConnection = true)
                .ConfigureConnection(settings => settings.WriteConcern = WriteConcern.W3);
            MongoDbContextData result = mongoDatabaseBuilder.Build();

            // Assert
            Assert.Equal("Test", result.Client.Settings.ApplicationName);
            Assert.True(result.Client.Settings.DirectConnection);
            Assert.Equal(WriteConcern.W3, result.Client.Settings.WriteConcern);
        }

        [Fact]
        public void ConfigureConnection_SetWrongConnectionSettings_ThrowsExceptionDuringBuild()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

            // Act
            mongoDatabaseBuilder.ConfigureConnection(
                settings => settings.HeartbeatInterval = TimeSpan.FromSeconds(-20));
            Action buildMongoDbContext = () => mongoDatabaseBuilder.Build();

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(buildMongoDbContext);
        }

        [Fact]
        public void ConfigureConnection_NoSpecificConnectionSettingsSet_DefaultConnectionSettingsSet()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

            // Act
            mongoDatabaseBuilder
                .ConfigureConnection(settings => settings.ApplicationName = "Test");
            MongoDbContextData result = mongoDatabaseBuilder.Build();

            // Assert
            Assert.Equal(ReadPreference.Primary, result.Client.Settings.ReadPreference);
            Assert.Equal(ReadConcern.Majority, result.Client.Settings.ReadConcern);
            Assert.Equal(WriteConcern.WMajority.With(journal: true),
                         result.Client.Settings.WriteConcern);
        }

        [Fact]
        public void ConfigureConnection_NoConnectionSettingsSet_DefaultConnectionSettingsSet()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

            // Act
            MongoDbContextData result = mongoDatabaseBuilder.Build();

            // Assert
            Assert.Equal(ReadPreference.Primary, result.Client.Settings.ReadPreference);
            Assert.Equal(ReadConcern.Majority, result.Client.Settings.ReadConcern);
            Assert.Equal(WriteConcern.WMajority.With(journal: true),
                         result.Client.Settings.WriteConcern);
        }

        #endregion

        #region RegisterConventionPack Tests

        [Fact]
        public void RegisterConventionPack_RegisterOneSpecificConventionPack_RegisteredSuccessfully()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

            mongoDatabaseBuilder.RegisterConventionPack(
                "camelCase", new ConventionPack
                {
                    new EnumRepresentationConvention(BsonType.String),
                    new CamelCaseElementNameConvention()
                }, t => true);

            // Act
            MongoDbContextData result = mongoDatabaseBuilder.Build();

            // Assert
            IEnumerable<IConvention> conventions = ConventionRegistry.Lookup(typeof(string)).Conventions;
            Assert.NotNull(conventions.OfType<EnumRepresentationConvention>().FirstOrDefault(c => c.Representation == BsonType.String));
            Assert.NotNull(conventions.OfType<CamelCaseElementNameConvention>().FirstOrDefault());
        }

        [Fact]
        public void RegisterConventionPack_RegisterEqualConventionPacksTwoTimes_OneConventionPackRegistered()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

            mongoDatabaseBuilder.RegisterConventionPack(
                "duplicateTestConventionPack", new ConventionPack
                {
                    new DuplicateTestConvention1(),
                    new DuplicateTestConvention2()
                }, t => true);

            mongoDatabaseBuilder.RegisterConventionPack(
                "duplicateTestConventionPack", new ConventionPack
                {
                    new DuplicateTestConvention2(),
                    new DuplicateTestConvention1()
                }, t => true);

            // Act
            MongoDbContextData result = mongoDatabaseBuilder.Build();

            // Assert
            IEnumerable<IConvention> conventions = ConventionRegistry
                .Lookup(typeof(string)).Conventions;
            int duplicateTestConventionCount1 = conventions
                .Count(convention => convention.Name == nameof(DuplicateTestConvention1));
            int duplicateTestConventionCount2 = conventions
                .Count(convention => convention.Name == nameof(DuplicateTestConvention2));

            Assert.Equal(1, duplicateTestConventionCount1);
            Assert.Equal(1, duplicateTestConventionCount2);
        }

        [Fact]
        public void RegisterConventionPack_RegisterDifferingConventionPacksTwoTimes_ThrowsException()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

            mongoDatabaseBuilder.RegisterConventionPack(
                "differingTestConventionPack", new ConventionPack
                {
                    new DifferingTestConvention1(),
                    new DifferingTestConvention2()
                }, t => true);

            mongoDatabaseBuilder.RegisterConventionPack(
                "differingTestConventionPack", new ConventionPack
                {
                    new DifferingTestConvention1(),
                }, t => true);

            // Act
            Action differingRegistration = () => mongoDatabaseBuilder.Build();

            // Assert
            Assert.Throws<Exception>(differingRegistration);
        }

        [Fact]
        public void RegisterConventionPack_RegisterUnequalConventionPacksTwoTimes_ThrowsException()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

            mongoDatabaseBuilder.RegisterConventionPack(
                "unequalTestConventionPack", new ConventionPack
                {
                    new DifferingTestConvention1(),
                    new DifferingTestConvention2()
                }, t => true);

            mongoDatabaseBuilder.RegisterConventionPack(
                "unequalTestConventionPack", new ConventionPack
                {
                    new DifferingTestConvention1(),
                    new DifferingTestConvention2(),
                    new DifferingTestConvention3()
                }, t => true);

            // Act
            Action differingRegistration = () => mongoDatabaseBuilder.Build();

            // Assert
            Assert.Throws<Exception>(differingRegistration);
        }

        [Fact]
        [Priority(0)]
        public void RegisterConventionPack_ConventionPackNotRegistered_ConventionPacksNotRegistered()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

            // Act
            MongoDbContextData result = mongoDatabaseBuilder.Build();

            // Assert
            IEnumerable<IConvention> conventions = ConventionRegistry.Lookup(typeof(string)).Conventions;
            int enumRepConvention = conventions.Count(convention => convention.Name == "EnumRepresentation");
            int camelCaseConvention = conventions.Count(convention => convention.Name == "CamelCaseElementName");

            Assert.Equal(0, enumRepConvention);
            Assert.Equal(0, camelCaseConvention);
        }

        [Fact]
        public void RegisterConventionPack_NullConventionPackRegistered_ThrowsException()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

            mongoDatabaseBuilder.RegisterConventionPack("nullConventionPack", null, t => true);

            // Act
            Action registrationAction = () => mongoDatabaseBuilder.Build();

            // Assert
            Assert.Throws<ArgumentNullException>(registrationAction);
        }

        [Fact]
        public void RegisterConventionPack_RegisterOneSpecificConventionPackWithoutName_ThrowsException()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

            mongoDatabaseBuilder.RegisterConventionPack(
                null, new ConventionPack
                {
                    new EnumRepresentationConvention(BsonType.String),
                    new CamelCaseElementNameConvention()
                }, t => true);

            // Act
            Action registrationAction = () => mongoDatabaseBuilder.Build();

            // Assert
            Assert.Throws<ArgumentNullException>(registrationAction);
        }

        [Fact]
        public void RegisterImmutableConventionPack_RegisteredSuccessfully()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);
            mongoDatabaseBuilder.RegisterImmutableConventionPack();

            // Act
            mongoDatabaseBuilder.Build();

            // Assert
            IEnumerable<IConvention> conventions = ConventionRegistry.Lookup(typeof(string)).Conventions;
            Assert.NotNull(conventions.OfType<ImmutableConvention>().FirstOrDefault());
            Assert.NotNull(conventions.OfType<IgnoreExtraElementsConvention>().FirstOrDefault());
        }

        [Fact]
        public void RegisterDefaultConventionPack_RegisteredSuccessfully()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);
            mongoDatabaseBuilder.RegisterDefaultConventionPack();

            // Act
            MongoDbContextData result = mongoDatabaseBuilder.Build();

            // Assert
            IEnumerable<IConvention> conventions = ConventionRegistry.Lookup(typeof(string)).Conventions;
            Assert.NotNull(conventions.OfType<EnumRepresentationConvention>().FirstOrDefault(c => c.Representation == BsonType.String));
            Assert.NotNull(conventions.OfType<IgnoreExtraElementsConvention>().FirstOrDefault());
            Assert.NotNull(conventions.OfType<ImmutableConvention>().FirstOrDefault());
        }

        #endregion

        #region RegisterSerializer Tests

        [Fact]
        public void RegisterSerializer_RegisterSpecificSerializer_RegisteredSuccessfully()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);
            var arrangedSerializer = new SimpleStringRegisteredSerializer();
            mongoDatabaseBuilder.RegisterSerializer<SimpleString>(arrangedSerializer);

            // Act
            mongoDatabaseBuilder.Build();

            // Assert
            IBsonSerializer<SimpleString> registeredSerializer =
                BsonSerializer.LookupSerializer<SimpleString>();

            Assert.True(registeredSerializer is SimpleStringRegisteredSerializer);
        }

        [Fact]
        public void RegisterSerializer_RegisterNoSpecificSerializer_DefaultSerializerRegistered()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

            // Act
            mongoDatabaseBuilder.Build();

            // Assert
            IBsonSerializer<NoSerializerRegistered> registeredSerializer =
                BsonSerializer.LookupSerializer<NoSerializerRegistered>();

            Assert.True(registeredSerializer is
                BsonClassMapSerializer<NoSerializerRegistered>);
        }

        [Fact]
        public void RegisterSerializer_RegisterSameSerializerTwoTimesForSameType_SerializerRegisteredOnce()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);
            var firstSerializer = new DuplicateRegisteredSerializer();
            var secondSerializer = new DuplicateRegisteredSerializer();

            mongoDatabaseBuilder.RegisterSerializer<DuplicateType>(firstSerializer);
            mongoDatabaseBuilder.RegisterSerializer<DuplicateType>(secondSerializer);

            // Act
            mongoDatabaseBuilder.Build();

            // Assert
            IBsonSerializer<DuplicateType> registeredSerializer =
                BsonSerializer.LookupSerializer<DuplicateType>();

            Assert.True(registeredSerializer is DuplicateRegisteredSerializer);
        }

        [Fact]
        public void RegisterSerializer_RegisterDifferentSerializerTwoTimesForSameType_ThrowsException()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);
            var originalSerializer = new DuplicateRegisteredSerializer();
            var differentSerializer = new DifferentRegisteredSerializer();

            mongoDatabaseBuilder.RegisterSerializer<DuplicateType>(originalSerializer);
            mongoDatabaseBuilder.RegisterSerializer<DuplicateType>(differentSerializer);

            // Act
            Action registerSerializers = () => mongoDatabaseBuilder.Build();

            // Assert
            Assert.Throws<BsonSerializationException>(registerSerializers);
        }

        [Fact]
        public void RegisterSerializer_RegisterNull_ThrowsException()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

            mongoDatabaseBuilder.RegisterSerializer<NullTestType>(null);

            // Act
            Action registerSerializers = () => mongoDatabaseBuilder.Build();

            // Assert
            Assert.Throws<ArgumentNullException>(registerSerializers);
        }

        #endregion

        #region DisableTableScan Tests

        [Fact]
        public void DisableTableScan_SetMongoServerTableScanToDisabled_MongoServerTableScanIsDisabled()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

            // Act
            mongoDatabaseBuilder.ConfigureDatabase(db => db.Client.DisableTableScan());
            MongoDbContextData result = mongoDatabaseBuilder.Build();

            // Assert
            Assert.True(result.Client.IsTableScanDisabled());
        }

        #endregion

        #region ConfigureCollection Tests

        [Fact]
        public void ConfigureCollection_SetDifferentSettingsToCollection_CollectionConfiguredSuccessfully()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);

            // Act
            mongoDatabaseBuilder.ConfigureCollection(new FooCollectionConfiguration());
            MongoDbContextData result = mongoDatabaseBuilder.Build();

            // Assert
            IMongoCollection<Foo> collection = result.GetCollection<Foo>();

            IEnumerable<BsonClassMap> classMaps = BsonClassMap.GetRegisteredClassMaps();
                
            Snapshot.Match(new
            {
                CollectionName = collection.CollectionNamespace.CollectionName,
                Settings = collection.Settings,
                Indexes = collection.Indexes.List().ToList(),
                ClassMaps = classMaps.Select(map => new {
                    Name = map.Discriminator,
                    IdMemberMap = new {
                        map.IdMemberMap?.ElementName,
                        map.IdMemberMap?.MemberName },
                    AllMemberMaps = map.AllMemberMaps.Select(amm =>
                        new { amm.ElementName, amm.MemberName }),
                    IgnoreExtraElements = map.IgnoreExtraElements
                })
            });
        }

        #endregion

        #region AddInstrumentation Tests

        [Fact]
        public void AddInstrumentation_Command_ActivityCreated()
        {
            // Arrange
            var mongoDatabaseBuilder = new MongoDatabaseBuilder(_mongoOptions);
            mongoDatabaseBuilder.AddInstrumentation();

            // Act
            MongoDbContextData result = mongoDatabaseBuilder.Build();

            // Assert
            result.Client.Settings.ClusterConfigurator.Should().NotBeNull();
        }

        #endregion

        #region Private Helpers

        private class DuplicateTestConvention1 : ConventionBase
        {
            public DuplicateTestConvention1() : base(nameof(DuplicateTestConvention1))
            {
            }
        }

        private class DuplicateTestConvention2 : ConventionBase
        {
            public DuplicateTestConvention2() : base(nameof(DuplicateTestConvention2))
            {
            }
        }

        private class DifferingTestConvention1 : ConventionBase
        {
            public DifferingTestConvention1() : base(nameof(DifferingTestConvention1))
            {
            }
        }

        private class DifferingTestConvention2 : ConventionBase
        {
            public DifferingTestConvention2() : base(nameof(DifferingTestConvention2))
            {
            }
        }

        private class DifferingTestConvention3 : ConventionBase
        {
            public DifferingTestConvention3() : base(nameof(DifferingTestConvention3))
            {
            }
        }

        private class SimpleString {}
        private class NoSerializerRegistered {}
        private class NullTestType {}
        private class DuplicateType {}

        private class SimpleStringRegisteredSerializer : IBsonSerializer<SimpleString>
        {
            public Type ValueType => typeof(string);

            public SimpleString Deserialize(
                BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                return new SimpleString();
            }

            public void Serialize(
               BsonSerializationContext context, BsonSerializationArgs args, SimpleString value)
            {
            }

            public void Serialize(
                BsonSerializationContext context, BsonSerializationArgs args, object value)
            {
            }

            object IBsonSerializer.Deserialize(
                BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                return string.Empty;
            }
        }

        private class DuplicateRegisteredSerializer : IBsonSerializer<DuplicateType>
        {
            public Type ValueType => typeof(string);

            public DuplicateType Deserialize(
                BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                return new DuplicateType();
            }

            public void Serialize(
               BsonSerializationContext context, BsonSerializationArgs args, DuplicateType value)
            {
            }

            public void Serialize(
                BsonSerializationContext context, BsonSerializationArgs args, object value)
            {
            }

            object IBsonSerializer.Deserialize(
                BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                return string.Empty;
            }
        }

        private class DifferentRegisteredSerializer : IBsonSerializer<DuplicateType>
        {
            public Type ValueType => typeof(string);

            public DuplicateType Deserialize(
                BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                return new DuplicateType();
            }

            public void Serialize(
               BsonSerializationContext context, BsonSerializationArgs args, DuplicateType value)
            {
            }

            public void Serialize(
                BsonSerializationContext context, BsonSerializationArgs args, object value)
            {
            }

            object IBsonSerializer.Deserialize(
                BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                return string.Empty;
            }
        }

        #endregion  
    }
}
