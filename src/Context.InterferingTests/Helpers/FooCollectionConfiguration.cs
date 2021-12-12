using MongoDB.Driver;

namespace MongoDB.Extensions.Context.InterferingTests.Helpers
{
    public class FooCollectionConfiguration : IMongoCollectionConfiguration<Foo>
    {
        public void OnConfiguring(IMongoCollectionBuilder<Foo> mongoCollectionBuilder)
        {
            mongoCollectionBuilder
                .WithCollectionName("TheArtOfFoo")
                .AddBsonClassMap<Foo>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(c => c.FooId);
                })
                .WithCollectionSettings(ConfigureSettings)
                .WithCollectionConfiguration(ConfigureIndexes);
        }

        private void ConfigureSettings(MongoCollectionSettings settings)
        {
            settings.ReadConcern = ReadConcern.Available;
            settings.WriteConcern = WriteConcern.WMajority.With(journal: true);
            settings.ReadPreference = ReadPreference.Primary;
        }

        private void ConfigureIndexes(IMongoCollection<Foo> auditCollection)
        {
            var nameIndex = new CreateIndexModel<Foo>(
                Builders<Foo>.IndexKeys.Ascending(foo => foo.Name),
                    new CreateIndexOptions {
                        Name = "Name_case_insensitive",
                        Unique = false,
                        Background = true,
                        Collation = new Collation(locale: "en",
                            strength: CollationStrength.Secondary)
                    });

            var numberIndex = new CreateIndexModel<Foo>(
                Builders<Foo>.IndexKeys.Ascending(foo => foo.Number),
                    new CreateIndexOptions { Unique = true });

            auditCollection.Indexes.CreateMany(new[] { nameIndex, numberIndex });
        }
    }
}
