using MongoDB.Driver;

namespace Mongo.DB.Extensions.Context.Oidc;

public static class MongoClientSettingsExtensions
{
    public static void AddOidcAuthentication(this MongoClientSettings mongoClientSettings, List<string> scopes)
    {
        mongoClientSettings.Credential = MongoCredential.CreateOidcCredential(new MongoOidcCallback(scopes));
    }
}
