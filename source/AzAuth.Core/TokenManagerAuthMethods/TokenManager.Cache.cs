using Microsoft.Identity.Client;

namespace PipeHow.AzAuth;

internal static partial class TokenManager
{
    /// <summary>
    /// Gets token noninteractively from existing named token cache.
    /// </summary>
    internal static AzToken GetTokenFromCache(string resource, string[] scopes, string? claims, string? clientId, string? tenantId, string tokenCache, string rootDir, string? username, bool useUnprotectedTokenCache, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenFromCacheAsync(resource, scopes, claims, clientId, tenantId, tokenCache, rootDir, username, useUnprotectedTokenCache, cancellationToken));

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
        string? username,
        bool useUnprotectedTokenCache,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();

        if (string.IsNullOrWhiteSpace(tokenCache))
        {
            throw new ArgumentNullException(nameof(tokenCache), "The specified token cache cannot be null or empty!");
        }

        bool resolvedUsername = false;
        try
        {
            if (username is null)
            {
                var accounts = CacheManager.GetAccounts(tokenCache, rootDir, useUnprotectedTokenCache, cancellationToken);
                if (accounts.Length == 0)
                {
                    throw new InvalidOperationException("No accounts found in the specified token cache.");
                }
                else if (accounts.Length > 1)
                {
                    throw new InvalidOperationException("Multiple accounts found in the token cache, please specify -Username!");
                }

                username = accounts[0];
                resolvedUsername = true;
            }
            return await CacheManager.GetTokenFromCacheSilentAsync(tokenCache, rootDir, clientId, tenantId, fullScopes, claims, username, useUnprotectedTokenCache, cancellationToken);
        }
        catch (MsalUiRequiredException ex) when (ex.Classification == UiRequiredExceptionClassification.AcquireTokenSilentFailed && ex.Message.Contains("No Refresh Token found in the cache"))
        {
            throw new InvalidOperationException("No refresh token was found in the cache, it may have expired.", ex);
        }
        catch (MsalUiRequiredException ex) when (ex.Classification == UiRequiredExceptionClassification.AcquireTokenSilentFailed && ex.Message.Contains("No account was found"))
        {
            string message = resolvedUsername ? $"Only the account '{username}' was found in the cache, but no token could be acquired for it." : $"The specified account '{username}' was not found in the cache.";
            throw new InvalidOperationException(message, ex);
        }
        catch (MsalUiRequiredException ex) when (ex.Classification == UiRequiredExceptionClassification.AcquireTokenSilentFailed)
        {
            throw new InvalidOperationException("No token could be acquired silently from the cache. This can happen if the token has expired, an account was not found in the cache, or if the cache was created unprotected. For unprotected caches, ensure the parameter -UseUnprotectedTokenCache is used to retrieve the token. See inner exception for details.", ex);
        }
        catch (MsalServiceException ex) when (ex.Message.Contains("Please do not use the /consumers endpoint to serve this request."))
        {
            throw new InvalidOperationException("The account used is a personal account and cannot use a general endpoint. Please specify the tenant using the parameter -Tenant.", ex);
        }
    }
}