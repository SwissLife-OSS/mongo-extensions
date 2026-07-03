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
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public MongoAuthType AuthType { get; set; }
        public List<string>? OidcScopes { get; set; }
    }
}
