using MongoDB.Extensions.Migration;

namespace MongoMigrationTest.Integration.Scenario2;

public record TestEntityForUp(string Id, string Foo) : IVersioned
{
    public int Version { get; set; }
}
