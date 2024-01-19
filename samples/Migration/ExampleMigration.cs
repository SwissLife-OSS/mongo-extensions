using MongoDB.Bson;
using MongoDB.Extensions.Migration;

namespace Migration;

public class ExampleMigration : IMigration
{
    public int Version => 1;

    public void Up(BsonDocument document)
    {
        document["Name"] += " Migrated up to 1";
    }

    public void Down(BsonDocument document)
    {
        document["Name"] += " Migrated down to 0";
    }
}
