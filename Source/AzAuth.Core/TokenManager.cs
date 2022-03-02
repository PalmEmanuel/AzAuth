using AzAuth.Core.Models;
using Azure.Core;
using Azure.Identity;

namespace AzAuth.Core
{
    public static class TokenManager
    {
        /// <summary>
        /// Gets token noninteractively.
        /// </summary>
        public static AzToken GetToken(string resource, string[] scopes, string? claims = null, string? tenantId = null)
        {
            var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();

            // Create our own credential chain because we want to change the order
            var credential = new ChainedTokenCredential(
                new EnvironmentCredential(),
                new SharedTokenCacheCredential(),
                new AzurePowerShellCredential(),
                new AzureCliCredential(),
                new VisualStudioCodeCredential(),
                new VisualStudioCredential()
            );

            var tokenRequestContext = new TokenRequestContext(fullScopes, claims: claims, tenantId: tenantId);
            AccessToken token = credential.GetToken(tokenRequestContext);
            return new AzToken(token.Token, resource, scopes, token.ExpiresOn);
        }

        /// <summary>
        /// Gets token interactively.
        /// </summary>
        public static AzToken GetTokenInteractive(string resource, string[] scopes, string? claims = null, string? tenantId = null)
        {
            var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();

            var tokenRequestContext = new TokenRequestContext(fullScopes, claims: claims, tenantId: tenantId);
            AccessToken token = new InteractiveBrowserCredential().GetToken(tokenRequestContext);
            return new AzToken(token.Token, resource, scopes, token.ExpiresOn);
        }

        /// <summary>
        /// Gets token as managed identity.
        /// </summary>
        public static AzToken GetTokenManagedIdentity(string resource, string[] scopes, string? claims = null, string? tenantId = null)
        {
            var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();

            var tokenRequestContext = new TokenRequestContext(fullScopes, claims: claims, tenantId: tenantId);
            AccessToken token = new ManagedIdentityCredential().GetToken(tokenRequestContext);
            return new AzToken(token.Token, resource, scopes, token.ExpiresOn);
        }
    }
}