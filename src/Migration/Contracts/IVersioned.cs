namespace MongoDB.Extensions.Migration;

public interface IVersioned
{
    int Version { get; set; }
}