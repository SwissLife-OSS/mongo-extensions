using MongoDB.Bson;

namespace MongoDB.Extensions.Migration;

public interface IMigration
{
    int Version { get; }
    void Up(BsonDocument document);
    void Down(BsonDocument document);
}
