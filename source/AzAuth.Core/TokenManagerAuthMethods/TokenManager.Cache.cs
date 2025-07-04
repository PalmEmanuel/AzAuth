using Microsoft.Identity.Client;

namespace PipeHow.AzAuth;

internal static partial class TokenManager
{
    /// <summary>
    /// Gets token noninteractively from existing named token cache.
    /// </summary>
    internal static AzToken GetTokenFromCache(string resource, string[] scopes, string? claims, string? clientId, string? tenantId, string tokenCache, string rootDir, string username, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenFromCacheAsync(resource, scopes, claims, clientId, tenantId, tokenCache, rootDir, username, cancellationToken));

    /// <summary>
    /// Gets token noninteractively from existing named token cache.
    /// </summary>
    internal static async Task<AzToken> GetTokenFromCacheAsync(
        string resource,
        string[] scopes,
        string? claims,
        string? clientId,
        string? tenantId,
        string tokenCache,
        string rootDir,
        string username,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();

        if (string.IsNullOrWhiteSpace(tokenCache))
        {
            throw new ArgumentNullException(nameof(tokenCache), "The specified token cache cannot be null or empty!");
        }

        try
        {
            return await CacheManager.GetTokenFromCacheSilentAsync(tokenCache, rootDir, clientId, tenantId, fullScopes, claims, username, cancellationToken);
        }
        catch (MsalServiceException ex) when (ex.Message.Contains("Please do not use the /consumers endpoint to serve this request."))
        {
            throw new InvalidOperationException("The account used is a personal account and cannot use a general endpoint. Please specify the tenant using the parameter -TenantId.", ex);
        }
    }
}