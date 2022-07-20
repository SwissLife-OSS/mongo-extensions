using MongoDB.Bson;
using MongoDB.Extensions.Migration;

namespace Migration.Tests;

public class TestMigration3 : IMigration
{
    public int Version { get; } = 3;

    public void Up(BsonDocument document)
    {
        document["Foo"] += " Migrated Up to 3";
    }

    public void Down(BsonDocument document)
    {
        document["Foo"] += " Migrated Down to 2";
    }
}
