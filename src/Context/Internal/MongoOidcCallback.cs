using Azure.Core;
using Azure.Identity;
using MongoDB.Driver.Authentication.Oidc;

namespace MongoDB.Extensions.Context.Internal;

internal class MongoOidcCallback(List<string> scopes) : IOidcCallback
{
    private static readonly TimeSpan ExpirationBuffer = TimeSpan.FromMinutes(1);

    public OidcAccessToken GetOidcAccessToken(
        OidcCallbackParameters parameters,
        CancellationToken cancellationToken)
    {
        var credential = new DefaultAzureCredential();

        AccessToken accessToken = credential.GetToken(
            new TokenRequestContext(scopes.ToArray()), cancellationToken);

        return ToOidcAccessToken(accessToken);
    }

    public async Task<OidcAccessToken> GetOidcAccessTokenAsync(
        OidcCallbackParameters parameters,
        CancellationToken cancellationToken)
    {
        var credential = new DefaultAzureCredential();

        AccessToken accessToken = await credential.GetTokenAsync(
            new TokenRequestContext(scopes.ToArray()), cancellationToken);

        return ToOidcAccessToken(accessToken);
    }

    private static OidcAccessToken ToOidcAccessToken(AccessToken accessToken)
    {
        var expiresIn = accessToken.ExpiresOn - DateTimeOffset.UtcNow - ExpirationBuffer;
        return new(accessToken.Token, expiresIn);
    }
}
