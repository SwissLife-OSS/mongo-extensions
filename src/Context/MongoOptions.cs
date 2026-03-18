using System;
using System.Collections.Generic;

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
        public MongoAuthType AuthType { get; set; }
        public List<string>? OidcScopes { get; set; }
    }
}
