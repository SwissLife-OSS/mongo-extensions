using MongoDB.Extensions.Migration;

namespace MongoMigrationTest.Integration.Scenario1;

public record TestEntityForDown(string Id, string Foo, int Version) : IVersioned
{
    public int Version { get; set; }
}