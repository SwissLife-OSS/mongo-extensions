using Microsoft.Extensions.Configuration;

namespace MongoDB.Extensions.Context
{
    public static class MongoOptionsConfiguration
    {
        public static MongoOptions GetMongoOptions(
            this IConfiguration configuration, string mongoDbPath)
        {
            return configuration
                .GetSection(mongoDbPath)
                .Get<MongoOptions>()
                .Validate();
        }

        public static MongoOptions<TMongoDBContext> GetMongoOptions<TMongoDBContext>(
            this IConfiguration configuration, string mongoDbPath) where TMongoDBContext : IMongoDbContext
        {
            return configuration
                .GetSection(mongoDbPath)
                .GetMongoOptions<TMongoDBContext>();
        }

        public static MongoOptions<TMongoDBContext> GetMongoOptions<TMongoDBContext>(
            this IConfigurationSection section) where TMongoDBContext : IMongoDbContext
        {
            MongoOptions<TMongoDBContext> mongoOptions =
                section.Get<MongoOptions<TMongoDBContext>>();

            mongoOptions.Validate();

            return mongoOptions;
        }
    }
}
