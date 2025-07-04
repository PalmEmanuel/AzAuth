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

    /// <summary>
    /// Gets the default cache directory path.
    /// </summary>
    internal static string GetCacheRootDirectory() => $"{MsalCacheHelper.UserRootDirectory}/.IdentityService";

    private static async Task InitializeCacheManagerAsync(string cacheName, string rootDir, string? clientId, string? tenantId, CancellationToken cancellationToken)
    {
        var cacheDir = $"{rootDir ?? GetCacheRootDirectory()}/{cacheName}";
        
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
    internal static void CreateCacheIfNotExists(string cacheName, string rootDir, string? clientId, string? tenantId, CancellationToken cancellationToken) =>
        taskFactory.Run(() => CreateCacheIfNotExistsAsync(cacheName, rootDir, clientId, tenantId, cancellationToken));

    internal static async Task CreateCacheIfNotExistsAsync(string cacheName, string rootDir, string? clientId, string? tenantId, CancellationToken cancellationToken)
    {
        await InitializeCacheManagerAsync(cacheName, rootDir, clientId, tenantId, cancellationToken);
    }

    /// <summary>
    /// Get an access token interactively.
    /// </summary>
    internal static AzToken GetTokenInteractive(string cacheName, string rootDir, string? clientId, string? tenantId, IEnumerable<string> scopes, string? claims, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenInteractiveAsync(cacheName, rootDir, clientId, tenantId, scopes, claims, cancellationToken));

    internal static async Task<AzToken> GetTokenInteractiveAsync(
        string cacheName,
        string rootDir,
        string? clientId,
        string? tenantId,
        IEnumerable<string> scopes,
        string? claims,
        CancellationToken cancellationToken)
    {
        await InitializeCacheManagerAsync(cacheName, rootDir, clientId, tenantId, cancellationToken);

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
    internal static AzToken GetTokenDeviceCode(string cacheName, string rootDir, string? clientId, string? tenantId, IEnumerable<string> scopes, string? claims, BlockingCollection<string> loggingQueue, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenDeviceCodeAsync(cacheName, rootDir, clientId, tenantId, scopes, claims, loggingQueue, cancellationToken));

    internal static async Task<AzToken> GetTokenDeviceCodeAsync(
        string cacheName,
        string rootDir,
        string? clientId,
        string? tenantId,
        IEnumerable<string> scopes,
        string? claims,
        BlockingCollection<string> loggingQueue,
        CancellationToken cancellationToken)
    {
        await InitializeCacheManagerAsync(cacheName, rootDir, clientId, tenantId, cancellationToken);

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
    internal static void GetTokenFromCacheSilent(string cacheName, string rootDir, string? clientId, string? tenantId, IEnumerable<string> scopes, string? claims, string username, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenFromCacheSilentAsync(cacheName, rootDir, clientId, tenantId, scopes, claims, username, cancellationToken));

    internal static async Task<AzToken> GetTokenFromCacheSilentAsync(
        string cacheName,
        string rootDir,
        string? clientId,
        string? tenantId,
        IEnumerable<string> scopes,
        string? claims,
        string username,
        CancellationToken cancellationToken)
    {
        await InitializeCacheManagerAsync(cacheName, rootDir, clientId, tenantId, cancellationToken);

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
    internal static void ClearCache(string cacheName, string rootDir, CancellationToken cancellationToken) =>
        taskFactory.Run(() => ClearCacheAsync(cacheName, rootDir, cancellationToken));

    internal static async Task ClearCacheAsync(string cacheName, string rootDir, CancellationToken cancellationToken)
    {
        await InitializeCacheManagerAsync(cacheName, rootDir, null, null, cancellationToken);

        // Remove all accounts from cache async
        var accounts = await application!.GetAccountsAsync();

        // Task.Run to allow cancellationToken
        await Task.Run(() => Task.WhenAll(accounts.Select(application.RemoveAsync)), cancellationToken);

        // Unregister cache
        cacheHelper!.UnregisterCache(application.UserTokenCache);
    }

    internal static string[] GetAccounts(string? cacheName, string rootDir, CancellationToken cancellationToken = default) =>
        taskFactory.Run(() => GetAccountsAsync(cacheName, rootDir, cancellationToken));

    private static async Task<string[]> GetAccountsAsync(string? cacheName, string rootDir, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cacheName))
        {
            throw new ArgumentNullException("cacheName", "Cache handling could not be initialized!");
        }

        await InitializeCacheManagerAsync(cacheName!, rootDir, null, null, cancellationToken);

        var accounts = await application!.GetAccountsAsync();
        return [.. accounts.Select(a => a.Username ?? a.HomeAccountId.ObjectId)];
    }

    /// <summary>
    /// Gets all available token caches in the default cache directory.
    /// </summary>
    internal static TokenCacheInfo[] GetAvailableCaches(string? cacheFilter, string rootDir, bool includeAccountInfo = false, CancellationToken cancellationToken = default) =>
        taskFactory.Run(() => GetAvailableCachesAsync(cacheFilter, rootDir, includeAccountInfo, cancellationToken));

    private static async Task<TokenCacheInfo[]> GetAvailableCachesAsync(string? cacheFilter, string rootDir, bool includeAccountInfo, CancellationToken cancellationToken)
    {
        var caches = new List<TokenCacheInfo>();

        // If no root directory is specified, use the default cache directory
        rootDir ??= GetCacheRootDirectory();

        if (!Directory.Exists(rootDir))
        {
            return [];
        }

        // Get either all cache directories, or the one matching the filter
        var cacheDirectories = Directory.GetDirectories(rootDir,
            string.IsNullOrWhiteSpace(cacheFilter) ? "*" : cacheFilter,
            SearchOption.TopDirectoryOnly);

        foreach (var cacheDir in cacheDirectories)
        {
            var cacheName = Path.GetFileName(cacheDir);
            var cacheInfo = new TokenCacheInfo
            {
                Name = cacheName,
                Path = cacheDir,
                CreatedDate = Directory.GetCreationTime(cacheDir),
                LastModified = Directory.GetLastWriteTime(cacheDir)
            };

            // Try to get account information if requested and possible
            if (includeAccountInfo)
            {
                var accounts = await GetAccountsAsync(cacheName, rootDir, cancellationToken);
                cacheInfo.AccountCount = accounts.Length;
                cacheInfo.Accounts = accounts;
                cacheInfo.AccountInfoChecked = true;
            }

            caches.Add(cacheInfo);
        }

        return [.. caches];
    }

    /// <summary>
    /// Deletes removes a cache directory and all its files. Use with caution as this may break other applications.
    /// </summary>
    internal static void RemoveCache(string cacheName, string rootDir, CancellationToken cancellationToken) =>
        taskFactory.Run(() => RemoveCacheAsync(cacheName, rootDir, cancellationToken));

    internal static async Task RemoveCacheAsync(string cacheName, string rootDir, CancellationToken cancellationToken)
    {
        var cacheDir = Path.Combine(rootDir, cacheName);
        
        // Safety checks
        // Prevent dangerous path characters and patterns
        var dangerousPatterns = new[] { "..", "\\", ":", "*", "?", "<", ">", "|" };
        if (dangerousPatterns.Any(pattern => cacheName.Contains(pattern)))
        {
            throw new ArgumentException($"Cache name '{cacheName}' contains invalid characters that could be dangerous to attempt to delete.", nameof(cacheName));
        }
        
        // Ensure it exists
        if (!Directory.Exists(cacheDir))
        {
            throw new DirectoryNotFoundException($"Cache directory '{cacheDir}' does not exist.");
        }

        // Check directory size (prevent deletion of massive directories)
        var (fileCount, totalSize) = GetDirectoryInfo(cacheDir);
        // Set large limits for a typical cache, but it should prevent accidental deletion of large directories
        const long MaxCacheSizeBytes = 5 * 1024 * 1024;
        const int MaxCacheFiles = 10;

        if (totalSize > MaxCacheSizeBytes)
        {
            throw new InvalidOperationException($"Directory '{cacheDir}' is too large ({totalSize / (1024 * 1024)} MB) to be a typical cache. Deletion aborted for safety.");
        }

        if (fileCount > MaxCacheFiles)
        {
            throw new InvalidOperationException($"Directory '{cacheDir}' contains too many files ({fileCount}) to be a typical cache. Deletion aborted for safety.");
        }

        try
        {
            // First try to clear the cache using MSAL to clean up properly
            await ClearCacheAsync(cacheName, rootDir, cancellationToken);
        }
        catch
        {
            // If MSAL clearing fails, we'll still try to delete the files
            // This could happen if the cache is corrupted or inaccessible
        }

        try
        {
            // Delete the entire cache directory and all its contents
            Directory.Delete(cacheDir, recursive: true);
        }
        catch (UnauthorizedAccessException)
        {
            throw new InvalidOperationException($"Access denied when trying to delete cache directory '{cacheDir}'. The cache may be in use by another process.");
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Unable to delete cache directory '{cacheDir}': {ex.Message}. The cache may be in use by another process.");
        }
    }

    /// <summary>
    /// Gets file count and total size of a directory.
    /// </summary>
    private static (int fileCount, long totalSize) GetDirectoryInfo(string directory)
    {
        try
        {
            var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            var totalSize = files.Sum(file => new FileInfo(file).Length);
            return (files.Length, totalSize);
        }
        catch
        {
            // If we can't read the directory info, assume it's small for safety
            return (0, 0);
        }
    }
}