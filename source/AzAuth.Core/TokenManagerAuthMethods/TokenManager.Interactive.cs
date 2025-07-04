using Azure.Core;
using Azure.Identity;

namespace PipeHow.AzAuth;

internal static partial class TokenManager
{
    /// <summary>
    /// Gets token interactively.
    /// </summary>
    internal static AzToken GetTokenInteractive(string resource, string[] scopes, string? claims, string? clientId, string? tenantId, string? tokenCache, string rootDir, int timeoutSeconds, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenInteractiveAsync(resource, scopes, claims, clientId, tenantId, tokenCache, rootDir, timeoutSeconds, cancellationToken));

    /// <summary>
    /// Gets token interactively.
    /// </summary>
    internal static async Task<AzToken> GetTokenInteractiveAsync(
        string resource,
        string[] scopes,
        string? claims,
        string? clientId,
        string? tenantId,
        string? tokenCache,
        string rootDir,
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        if (!string.IsNullOrWhiteSpace(tokenCache))
        {
            return await CacheManager.GetTokenInteractiveAsync(tokenCache!, rootDir, clientId, tenantId, fullScopes, claims, cancellationToken);
        }

        var options = new InteractiveBrowserCredentialOptions
        {
            ClientId = clientId
        };

        // Create a new credential
        credential = new InteractiveBrowserCredential(options);
        
        previousClientId = clientId;
        
        try
        {
            // Create a new cancellation token by combining a timeout with existing token
            using var timeoutSource = new CancellationTokenSource(timeoutSeconds * 1000);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token).Token;
            return await GetTokenAsync(tokenRequestContext, combinedToken);
        }
        catch (OperationCanceledException ex)
        {
            // Only the timeout is caught here, CTRL + C in PowerShell does not throw an error
            throw new OperationCanceledException($"Login timed out after {timeoutSeconds} seconds, configured by the TimeoutSeconds parameter.", ex);
        }
    }
}