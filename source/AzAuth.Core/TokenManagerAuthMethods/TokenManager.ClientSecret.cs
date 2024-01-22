using Azure.Core;
using Azure.Identity;

namespace PipeHow.AzAuth;

internal static partial class TokenManager
{
    /// <summary>
    /// Gets token with a client secret.
    /// </summary>
    internal static AzToken GetTokenClientSecret(string resource, string[] scopes, string? claims, string clientId, string tenantId, string clientSecret, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenClientSecretAsync(resource, scopes, claims, clientId, tenantId, clientSecret, cancellationToken));

    /// <summary>
    /// Gets token with a client secret.
    /// </summary>
    internal static async Task<AzToken> GetTokenClientSecretAsync(
        string resource,
        string[] scopes,
        string? claims,
        string clientId,
        string tenantId,
        string clientSecret,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        // Re-use the previous credential if client id didn't change
        if (credential is not ClientSecretCredential || previousClientId != clientId)
        {
            credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        }

        previousClientId = clientId;

        return await GetTokenAsync(tokenRequestContext, cancellationToken);
    }
}