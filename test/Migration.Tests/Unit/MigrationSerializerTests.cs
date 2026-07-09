using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Extensions.Migration;
using Xunit;

namespace Migration.Tests.Unit;

public class MigrationSerializerTests 

{
    public record RootEntity(ChildEntity? Child);

    public record ChildEntity(int Version, string Property) : IVersioned
    {
        public int Version { get; set; } = Version;
    }

    public class ChildEntityMigration : IMigration
    {
        public int Version => 1;

        public void Up(BsonDocument document)
        {
            document["Property"] = document["Property"].AsString + " (migrated)";
        }

        public void Down(BsonDocument document)
        {
        }
    }

    [Fact]
    public void Serialize_ContainsNull_SerializesCorrectly()
    {
        //ARRANGE
        var entityOption = new EntityOption(typeof(ChildEntity), 1, [new ChildEntityMigration()]);
        var migrationOption = new MigrationOption([entityOption]);
        BsonSerializer.RegisterSerializationProvider(
            new MigrationSerializerProvider(new MigrationContext(migrationOption, new NullLoggerFactory())));
        var entityToSerialize = new RootEntity(null);
        var stringWriter = new StringWriter();
        var writer = new JsonWriter(stringWriter);

        //ACT
        BsonSerializer.Serialize(writer, entityToSerialize);
        var result = stringWriter.ToString();

        //ASSERT
        result.Should().Be("""{ "Child" : null }""");
    }

    [Fact]
    public void Deserialize_ContainsNull_DeserializesCorrectly()
    {
        //ARRANGE
        var entityOption = new EntityOption(typeof(ChildEntity), 1, [new ChildEntityMigration()]);
        var migrationOption = new MigrationOption([entityOption]);
        BsonSerializer.RegisterSerializationProvider(
            new MigrationSerializerProvider(new MigrationContext(migrationOption, new NullLoggerFactory())));

        //ACT
        var document = BsonDocument.Parse("""{ "Child" : null }""");
        var entity = BsonSerializer.Deserialize<RootEntity>(document);

        //ASSERT
        entity.Should().NotBeNull();
        entity.Child.Should().BeNull();
    }

    [Fact]
    public void Serialize_ContainsNonNull_SerializesCorrectly()
    {
        //ARRANGE
        var entityOption = new EntityOption(typeof(ChildEntity), 1, [new ChildEntityMigration()]);
        var migrationOption = new MigrationOption([entityOption]);
        BsonSerializer.RegisterSerializationProvider(
            new MigrationSerializerProvider(new MigrationContext(migrationOption, new NullLoggerFactory())));
        var entityToSerialize = new RootEntity(new ChildEntity(0, "Test"));
        var stringWriter = new StringWriter();
        var writer = new JsonWriter(stringWriter);

        //ACT
        BsonSerializer.Serialize(writer, entityToSerialize);
        var result = stringWriter.ToString();

        //ASSERT
        //Migration happens on deserialization, so Property is not migrated
        result.Should().Be("""{ "Child" : { "Property" : "Test", "Version" : 1 } }""");
    }

    [Fact]
    public void Deserialize_ContainsNonNull_DeserializesCorrectly()
    {
        //ARRANGE
        var entityOption = new EntityOption(typeof(ChildEntity), 1, [new ChildEntityMigration()]);
        var migrationOption = new MigrationOption([entityOption]);
        BsonSerializer.RegisterSerializationProvider(
            new MigrationSerializerProvider(new MigrationContext(migrationOption, new NullLoggerFactory())));

        //ACT
        var document = BsonDocument.Parse("""{ "Child" : { "Property" : "Test", "Version" : 0 } }""");
        var entity = BsonSerializer.Deserialize<RootEntity>(document);

        //ASSERT
        entity.Should().NotBeNull();
        entity.Child.Should().NotBeNull();
        entity.Child.Property.Should().Be("Test (migrated)");
        entity.Child.Version.Should().Be(1);
    }
}