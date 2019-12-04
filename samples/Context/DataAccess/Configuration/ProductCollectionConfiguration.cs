using Domain.Models;
using MongoDB.Driver;
using MongoDB.Extensions.Context;

namespace DataAccess
{
    public class ProductCollectionConfiguration : IMongoCollectionConfiguration<Product>
    {
        public void Configure(IMongoCollectionBuilder<Product> mongoCollectionBuilder)
        {
            mongoCollectionBuilder
                .WithCollectionName("products")
                .AddBsonClassMap<Product>(cm => cm.AutoMap())
                .WithMongoCollectionSettings(settings => settings.ReadConcern = ReadConcern.Majority)
                .WithMongoCollectionSettings(settings => settings.ReadPreference = ReadPreference.Nearest)
                .WithMongoCollectionConfiguration(collection =>
                {
                    var nameIndex = new CreateIndexModel<Product>(
                        Builders<Product>.IndexKeys.Ascending(product => product.Name),
                        new CreateIndexOptions { Unique = true });

                    collection.Indexes.CreateOne(nameIndex);
                });
        }
    }
}
