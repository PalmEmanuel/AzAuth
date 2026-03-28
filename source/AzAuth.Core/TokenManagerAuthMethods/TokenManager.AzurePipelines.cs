using Azure.Core;
using Azure.Identity;

namespace PipeHow.AzAuth;

internal static partial class TokenManager
{
    /// <summary>
    /// Gets token using Azure Pipelines service connection OIDC.
    /// </summary>
    internal static AzToken GetTokenAzurePipelines(string resource, string[] scopes, string? claims, string clientId, string tenantId, string serviceConnectionId, string systemAccessToken, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenAzurePipelinesAsync(resource, scopes, claims, clientId, tenantId, serviceConnectionId, systemAccessToken, cancellationToken));

    /// <summary>
    /// Gets token using Azure Pipelines service connection OIDC.
    /// </summary>
    internal static async Task<AzToken> GetTokenAzurePipelinesAsync(
        string resource,
        string[] scopes,
        string? claims,
        string clientId,
        string tenantId,
        string serviceConnectionId,
        string systemAccessToken,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        // Re-use the previous credential if client id didn't change
        if (credential is not AzurePipelinesCredential || previousClientId != clientId)
        {
            credential = new AzurePipelinesCredential(tenantId, clientId, serviceConnectionId, systemAccessToken);
        }

        previousClientId = clientId;

        return await GetTokenAsync(tokenRequestContext, cancellationToken);
    }
}
