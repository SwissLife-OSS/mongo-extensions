using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver.Authentication.Oidc;
using System.Threading.Tasks;
using System.Threading;
using Azure.Identity;
using Azure.Core;

namespace MongoDB.Extensions.Context.Callbacks
{
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

}
