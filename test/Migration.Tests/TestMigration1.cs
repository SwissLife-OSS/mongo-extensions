using MongoDB.Bson;
using MongoDB.Extensions.Migration;

namespace Migration.Tests;

public class TestMigration1 : IMigration
{
    public int Version { get; } = 1;

    public void Up(BsonDocument document)
    {
        document["Foo"] += " Migrated Up to 1";
    }

    public void Down(BsonDocument document)
    {
        document["Foo"] += " Migrated Down to 0";
    }
}
