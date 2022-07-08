using MongoDB.Bson;

namespace MongoDB.Extensions.Migration;

public interface IMigration
{
    int Version { get; }
    public void Up(BsonDocument document);
    public void Down(BsonDocument document);
}