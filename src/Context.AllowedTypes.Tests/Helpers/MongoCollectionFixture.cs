using Squadron;
using Xunit;

namespace MongoDB.Extensions.Context.AllowedTypes.Tests;

public static class CollectionFixtureNames
{
    public const string MongoCollectionFixture = "MongoCollectionFixture";
}

[CollectionDefinition(CollectionFixtureNames.MongoCollectionFixture)]
public class MongoCollectionFixture
    : ICollectionFixture<MongoResource>
{
}
