using MongoDB.Extensions.Migration;

namespace Migration.Tests.Unit;

record TestEntity(int Id) : IVersioned
{
    public int Version { get; set; }
}
