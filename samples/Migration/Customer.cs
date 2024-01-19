using MongoDB.Extensions.Migration;

namespace Migration;

public record Customer(string Id, string Name) : IVersioned
{
    public int Version { get; set; }
}
