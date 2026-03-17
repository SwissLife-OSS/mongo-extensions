namespace MongoDB.Prime.Extensions.Tests;

public class Foo
{
    public Foo(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; set; }
    public string Name { get; set; }
}
