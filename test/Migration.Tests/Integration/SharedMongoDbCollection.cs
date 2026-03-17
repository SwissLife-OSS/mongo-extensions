using Squadron;
using Xunit;

namespace MongoMigrationTest.Integration;

[CollectionDefinition("SharedMongoDbCollection")]
public class SharedMongoDbCollection : ICollectionFixture<MongoResource>
{
}