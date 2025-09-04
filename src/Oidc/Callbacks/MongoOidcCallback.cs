using Azure.Core;
using Azure.Identity;
using MongoDB.Driver.Authentication.Oidc;

namespace Mongo.DB.Extensions.Context.Oidc;

internal class MongoOidcCallback : IOidcCallback
{
    private readonly List<string> _scopes;

    public MongoOidcCallback(List<string> scopes)
    {
        _scopes = scopes;
    }

    public OidcAccessToken GetOidcAccessToken(OidcCallbackParameters parameters, CancellationToken cancellationToken)
    {
        var credential = new DefaultAzureCredential();

        var accessToken = credential.GetToken(new TokenRequestContext(_scopes.ToArray())).Token;

        return new(accessToken, expiresIn: null);
    }

    public async Task<OidcAccessToken> GetOidcAccessTokenAsync(OidcCallbackParameters parameters, CancellationToken cancellationToken)
    {
        var credential = new DefaultAzureCredential();

        var accessToken = await credential.GetTokenAsync(new TokenRequestContext(_scopes.ToArray()));

        return new(accessToken.Token, expiresIn: null);
    }
}
