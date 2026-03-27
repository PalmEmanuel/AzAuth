using Azure.Core;
using Azure.Identity;

namespace PipeHow.AzAuth;

internal static partial class TokenManager
{
    /// <summary>
    /// Gets token using the OAuth 2.0 authorization code flow.
    /// </summary>
    internal static AzToken GetTokenAuthorizationCode(string resource, string[] scopes, string? claims, string clientId, string tenantId, string clientSecret, string authorizationCode, Uri? redirectUri, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenAuthorizationCodeAsync(resource, scopes, claims, clientId, tenantId, clientSecret, authorizationCode, redirectUri, cancellationToken));

    /// <summary>
    /// Gets token using the OAuth 2.0 authorization code flow.
    /// </summary>
    internal static async Task<AzToken> GetTokenAuthorizationCodeAsync(
        string resource,
        string[] scopes,
        string? claims,
        string clientId,
        string tenantId,
        string clientSecret,
        string authorizationCode,
        Uri? redirectUri,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        AuthorizationCodeCredential cred;
        if (redirectUri is not null)
        {
            cred = new AuthorizationCodeCredential(tenantId, clientId, clientSecret, authorizationCode, new AuthorizationCodeCredentialOptions { RedirectUri = redirectUri });
        }
        else
        {
            cred = new AuthorizationCodeCredential(tenantId, clientId, clientSecret, authorizationCode);
        }

        credential = cred;
        previousClientId = clientId;

        return await GetTokenAsync(tokenRequestContext, cancellationToken);
    }
}
