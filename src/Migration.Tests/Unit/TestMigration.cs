using System;
using MongoDB.Bson;
using MongoDB.Extensions.Migration;

namespace Migration.Tests.Unit;

public class TestMigration : IMigration
{
    public int Version { get; } = 1;

    public void Up(BsonDocument document)
    {
        throw new Exception();
    }

    public void Down(BsonDocument document)
    {
        throw new Exception();
    }
}
