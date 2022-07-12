using MongoDB.Bson;
using MongoDB.Extensions.Migration;

namespace Migration.Tests;

public class TestMigration2 : IMigration
{
    public int Version { get; } = 2;

    public void Up(BsonDocument document)
    {
        document["Foo"] += " Migrated Up to 2";
    }

    public void Down(BsonDocument document)
    {
        document["Foo"] += " Migrated Down to 1";
    }
}
