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
            MongoOptions<TMongoDBContext> mongoOptions = configuration
                    .GetSection(mongoDbPath)
                    .Get<MongoOptions<TMongoDBContext>>();

            mongoOptions.Validate();

            return mongoOptions;
        }
    }
}
