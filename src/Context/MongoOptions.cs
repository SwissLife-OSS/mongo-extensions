using System;

namespace MongoDB.Extensions.Context
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
}
