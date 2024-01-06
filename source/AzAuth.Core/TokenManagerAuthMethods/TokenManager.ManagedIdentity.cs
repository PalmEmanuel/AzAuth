using Azure.Core;
using Azure.Identity;

namespace PipeHow.AzAuth;

internal static partial class TokenManager
{
    /// <summary>
    /// Gets token as a managed identity.
    /// </summary>
    internal static AzToken GetTokenManagedIdentity(string resource, string[] scopes, string? claims, string? clientId, string? tenantId, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenManagedIdentityAsync(resource, scopes, claims, clientId, tenantId, cancellationToken));

    /// <summary>
    /// Gets token as a managed identity.
    /// </summary>
    internal static async Task<AzToken> GetTokenManagedIdentityAsync(
        string resource,
        string[] scopes,
        string? claims,
        string? clientId,
        string? tenantId,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        // Re-use the previous managed identity credential if client id didn't change
        if (credential is not ManagedIdentityCredential || previousClientId != clientId)
        {
            credential = new ManagedIdentityCredential(clientId);
        }

        previousClientId = clientId;

        return await GetTokenAsync(tokenRequestContext, cancellationToken);
    }
}