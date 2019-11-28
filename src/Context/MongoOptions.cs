using System;

namespace MongoDB.Bootstrapper
{
    public class MongoOptions<TMongoDBContext>
        : MongoOptions where TMongoDBContext : IMongoDbContext
    {
    }

    public class MongoOptions
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public static class MongoOptionsExtension 
    {
        public static MongoOptions<TMongoDBContext> Validate<TMongoDBContext>(
            this MongoOptions<TMongoDBContext> mongoOptions) where TMongoDBContext : IMongoDbContext
        {
            if (mongoOptions == null)
            {
                throw new Exception(
                    $"The MongoDB options could not be found " +
                    $"within the configuration section.");
            }

            if (string.IsNullOrEmpty(mongoOptions.ConnectionString))
            {
                throw new Exception(
                    $"The connection string of the MongoDB configuration " +
                    $"could not be found within the configuration section. " +
                    $"Please verify that this section contains the " +
                    $"{nameof(MongoOptions<TMongoDBContext>.ConnectionString)} field.");
            }

            if (string.IsNullOrEmpty(mongoOptions.DatabaseName))
            {
                throw new Exception(
                    $"The database name of the MongoDB configuration " +
                    $"could not be found within the section " +
                    $"Please verify that this section contains the " +
                    $"{nameof(MongoOptions<TMongoDBContext>.DatabaseName)} field.");
            }

            return mongoOptions;
        }
    }
}
