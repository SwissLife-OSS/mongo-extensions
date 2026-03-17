using System;

namespace MongoDB.Extensions.Context;

public static class MongoOptionsExtensions
{
    public static MongoOptions<TMongoDBContext> Validate<TMongoDBContext>(
        this MongoOptions<TMongoDBContext>? mongoOptions)
        where TMongoDBContext : IMongoDbContext
    {
        Validate(mongoOptions as MongoOptions);

        return mongoOptions!;
    }

    public static MongoOptions Validate(this MongoOptions? mongoOptions)
    {
        if (mongoOptions == null)
        {
            throw new Exception(
                $"The MongoDB options for could not be found " +
                $"within the configuration section or the options are null.");
        }

        if (string.IsNullOrEmpty(mongoOptions.ConnectionString))
        {
            throw new Exception(
                $"The connection string of the MongoDB configuration " +
                $"could not be found within the configuration section. " +
                $"Please verify that this section contains the " +
                $"{nameof(MongoOptions.ConnectionString)} field.");
        }

        if (string.IsNullOrEmpty(mongoOptions.DatabaseName))
        {
            throw new Exception(
                $"The database name of the MongoDB configuration " +
                $"could not be found within the section " +
                $"Please verify that this section contains the " +
                $"{nameof(MongoOptions.DatabaseName)} field.");
        }

        if (mongoOptions.AuthType == MongoAuthType.Oidc &&
            (mongoOptions.OidcScopes == null || mongoOptions.OidcScopes.Count == 0))
        {
            throw new Exception(
                $"The OIDC scopes of the MongoDB configuration " +
                $"must be provided when {nameof(MongoOptions.AuthType)} " +
                $"is set to {nameof(MongoAuthType.Oidc)}. " +
                $"Please verify that the configuration section contains the " +
                $"{nameof(MongoOptions.OidcScopes)} field with at least one scope.");
        }

        return mongoOptions;
    }
}
