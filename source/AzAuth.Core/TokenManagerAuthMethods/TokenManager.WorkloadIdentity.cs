using Azure.Core;
using Azure.Identity;

namespace PipeHow.AzAuth;

internal static partial class TokenManager
{
    /// <summary>
    /// Gets token as a workload identity.
    /// </summary>
    internal static AzToken GetTokenWorkloadIdentity(string resource, string[] scopes, string? claims, string? clientId, string tenantId, string externalToken, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenWorkloadIdentityAsync(resource, scopes, claims, clientId, tenantId, externalToken, cancellationToken));

    /// <summary>
    /// Gets token as a workload identity.
    /// </summary>
    internal static async Task<AzToken> GetTokenWorkloadIdentityAsync(
        string resource,
        string[] scopes,
        string? claims,
        string? clientId,
        string tenantId,
        string externalToken,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        // Re-use the previous credential if client id didn't change
        if (credential is not ClientAssertionCredential || previousClientId != clientId)
        {
            credential = new ClientAssertionCredential(tenantId, clientId, () => externalToken);
        }

        previousClientId = clientId;

        return await GetTokenAsync(tokenRequestContext, cancellationToken);
    }
}