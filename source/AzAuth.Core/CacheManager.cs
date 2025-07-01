using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.VisualStudio.Threading;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;

namespace PipeHow.AzAuth;

internal static class CacheManager
{
    private static MsalCacheHelper? cacheHelper;
    private static IPublicClientApplication? application;
    private static readonly JoinableTaskFactory taskFactory = new(new JoinableTaskContext());

    // This is the well-known client id for the public Azure CLI client app
    private const string AzureCliClientId = "04b07795-8ddb-461a-bbee-02f9e1bf7b46";

    private static async Task InitializeCacheManagerAsync(string cacheName, string? clientId, string? tenantId, CancellationToken cancellationToken)
    {
        // This is the directory where MSAL stores its cache by default
        var cacheDir = $"{MsalCacheHelper.UserRootDirectory}/.IdentityService/{cacheName}";
        
        var storageProperties = new StorageCreationPropertiesBuilder(cacheName, cacheDir)
            .WithMacKeyChain("PipeHow.AzAuth", cacheName)
            .WithLinuxKeyring(cacheName, MsalCacheHelper.LinuxKeyRingDefaultCollection, "AzAuthTokenCache",
                new KeyValuePair<string, string>("Product", "PipeHow.AzAuth"),
                new KeyValuePair<string, string>("PipeHow.AzAuth", "1.0.0.0"))
            .Build();

        // Task.Run to use cancellationToken
        cacheHelper = await Task.Run(() => MsalCacheHelper.CreateAsync(storageProperties), cancellationToken);

        application = PublicClientApplicationBuilder.Create(clientId ?? AzureCliClientId)
           .WithAuthority(AzureCloudInstance.AzurePublic, tenantId ?? "organizations") // Use tenant if available, otherwise personal accounts break
           .WithDefaultRedirectUri() // http://localhost on .NET core
           .Build();

        cacheHelper.RegisterCache(application.UserTokenCache);
        cacheHelper.VerifyPersistence();
    }

    /// <summary>
    /// Creates a token cache and registers it on disk.
    /// </summary>
    internal static void CreateCacheIfNotExists(string cacheName, string? clientId, string? tenantId, CancellationToken cancellationToken) =>
        taskFactory.Run(() => CreateCacheIfNotExistsAsync(cacheName, clientId, tenantId, cancellationToken));

    internal static async Task CreateCacheIfNotExistsAsync(string cacheName, string? clientId, string? tenantId, CancellationToken cancellationToken)
    {
        await InitializeCacheManagerAsync(cacheName, clientId, tenantId, cancellationToken);
    }

    /// <summary>
    /// Get an access token interactively.
    /// </summary>
    internal static AzToken GetTokenInteractive(string cacheName, string? clientId, string? tenantId, IEnumerable<string> scopes, string? claims, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenInteractiveAsync(cacheName, clientId, tenantId, scopes, claims, cancellationToken));

    internal static async Task<AzToken> GetTokenInteractiveAsync(
        string cacheName,
        string? clientId,
        string? tenantId,
        IEnumerable<string> scopes,
        string? claims,
        CancellationToken cancellationToken)
    {
        await InitializeCacheManagerAsync(cacheName, clientId, tenantId, cancellationToken);

        var tokenBuilder = application!.AcquireTokenInteractive(scopes);

        if (!string.IsNullOrWhiteSpace(claims))
        {
            tokenBuilder = tokenBuilder.WithClaims(claims);
        }

        var result = await tokenBuilder.ExecuteAsync(cancellationToken);

        // result.ClaimsPrincipal doesnt have the correct claims, so we extract claims from the token instead
        var resultClaims = new ClaimsDictionary();
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(result.AccessToken);
        foreach (var claim in jsonToken.Claims)
        {
            resultClaims.Add(claim.Type, claim.Value);
        }

        return new AzToken(
            result.AccessToken,
            result.Scopes.ToArray(),
            result.ExpiresOn,
            resultClaims,
            result.Account.Username ?? result.Account.HomeAccountId.ObjectId,
            result.TenantId
        );
    }

    /// <summary>
    /// Get an access token using a device code.
    /// </summary>
    internal static AzToken GetTokenDeviceCode(string cacheName, string? clientId, string? tenantId, IEnumerable<string> scopes, string? claims, BlockingCollection<string> loggingQueue, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenDeviceCodeAsync(cacheName, clientId, tenantId, scopes, claims, loggingQueue, cancellationToken));

    internal static async Task<AzToken> GetTokenDeviceCodeAsync(
        string cacheName,
        string? clientId,
        string? tenantId,
        IEnumerable<string> scopes,
        string? claims,
        BlockingCollection<string> loggingQueue,
        CancellationToken cancellationToken)
    {
        await InitializeCacheManagerAsync(cacheName, clientId, tenantId, cancellationToken);

        var tokenBuilder = application!.AcquireTokenWithDeviceCode(
            scopes,
            deviceCodeInfo =>
                Task.Run(() => {
                    loggingQueue.Add(deviceCodeInfo.Message, cancellationToken);
                    // Make sure to mark as completed, no more messages can be sent
                    loggingQueue.CompleteAdding();
                }, cancellationToken)
            );

        if (!string.IsNullOrWhiteSpace(claims))
        {
            tokenBuilder = tokenBuilder.WithClaims(claims);
        }

        var result = await tokenBuilder.ExecuteAsync(cancellationToken);

        // result.ClaimsPrincipal doesnt have all the correct claims, so we extract claims from the token instead
        var resultClaims = new ClaimsDictionary();
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(result.AccessToken);
        foreach (var claim in jsonToken.Claims)
        {
            resultClaims.Add(claim.Type, claim.Value);
        }

        return new AzToken(
            result.AccessToken,
            result.Scopes.ToArray(),
            result.ExpiresOn,
            resultClaims,
            result.Account.Username ?? result.Account.HomeAccountId.ObjectId,
            result.TenantId
        );
    }

    /// <summary>
    /// Get an access token silently from existing cache.
    /// </summary>
    internal static void GetTokenFromCacheSilent(string cacheName, string? clientId, string? tenantId, IEnumerable<string> scopes, string? claims, string username, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenFromCacheSilentAsync(cacheName, clientId, tenantId, scopes, claims, username, cancellationToken));

    internal static async Task<AzToken> GetTokenFromCacheSilentAsync(
        string cacheName,
        string? clientId,
        string? tenantId,
        IEnumerable<string> scopes,
        string? claims,
        string username,
        CancellationToken cancellationToken)
    {
        await InitializeCacheManagerAsync(cacheName, clientId, tenantId, cancellationToken);

        var tokenBuilder = application!.AcquireTokenSilent(scopes, username);

        if (!string.IsNullOrWhiteSpace(claims))
        {
            tokenBuilder = tokenBuilder.WithClaims(claims);
        }

        var result = await tokenBuilder.ExecuteAsync(cancellationToken);

        // result.ClaimsPrincipal doesnt have all the correct claims, so we extract claims from the token instead
        var resultClaims = new ClaimsDictionary();
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(result.AccessToken);
        foreach (var claim in jsonToken.Claims)
        {
            resultClaims.Add(claim.Type, claim.Value);
        }

        return new AzToken(
            result.AccessToken,
            result.Scopes.ToArray(),
            result.ExpiresOn,
            resultClaims,
            result.Account.Username ?? result.Account.HomeAccountId.ObjectId,
            result.TenantId
        );
    }

    /// <summary>
    /// Clears an existing cache and unregisters it from disk. The file may remain without tokens.
    /// </summary>
    internal static void ClearCache(string cacheName, CancellationToken cancellationToken) =>
        taskFactory.Run(() => ClearCacheAsync(cacheName, cancellationToken));

    internal static async Task ClearCacheAsync(string cacheName, CancellationToken cancellationToken)
    {
        await InitializeCacheManagerAsync(cacheName, null, null, cancellationToken);

        // Remove all accounts from cache async
        var accounts = await application!.GetAccountsAsync();

        // Task.Run to allow cancellationToken
        await Task.Run(() => Task.WhenAll(accounts.Select(application.RemoveAsync)), cancellationToken);

        // Unregister cache
        cacheHelper!.UnregisterCache(application.UserTokenCache);
    }

    internal static string[] GetAccounts(string? cacheName, CancellationToken cancellationToken = default) =>
        taskFactory.Run(() => GetAccountsAsync(cacheName, cancellationToken));

    private static async Task<string[]> GetAccountsAsync(string? cacheName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cacheName))
        {
            throw new ArgumentNullException("cacheName", "Cache handling could not be initialized!");
        }

        await InitializeCacheManagerAsync(cacheName!, null, null, cancellationToken);

        var accounts = await application!.GetAccountsAsync();
        return accounts.Select(a => a.Username ?? a.HomeAccountId.ObjectId).ToArray();
    }
}