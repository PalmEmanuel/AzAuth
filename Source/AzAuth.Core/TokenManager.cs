using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;

namespace PipeHow.AzAuth;

public static class TokenManager
{
    private static TokenCredential? credential;
    private static string? previousTenantId;
    private static string? previousClientId;
    private static readonly JoinableTaskFactory taskFactory = new(new JoinableTaskContext());

    internal static ILogger? Logger { get; set; }

    /// <summary>
    /// Gets token noninteractively.
    /// </summary>
    public static AzToken GetTokenNonInteractive(string resource, string[] scopes, string? claims, string? tenantId, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenNonInteractiveAsync(resource, scopes, claims, tenantId, cancellationToken));

    /// <summary>
    /// Gets token noninteractively.
    /// </summary>
    public static async Task<AzToken> GetTokenNonInteractiveAsync(
        string resource,
        string[] scopes,
        string? claims,
        string? tenantId,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();

        // Create our own credential chain because we want to change the order
        var sources = new List<TokenCredential>()
        {
            new EnvironmentCredential(),
            new AzurePowerShellCredential(),
            new AzureCliCredential(),
            new VisualStudioCodeCredential(),
            new VisualStudioCredential(),
            new SharedTokenCacheCredential()
        };

        // If user authenticated interactively in the same session and tenant didn't change, add it as the first option to find tokens from
        if (credential is InteractiveBrowserCredential && tenantId == previousTenantId)
        {
            sources.Insert(0, credential);
        }

        // Create a new credential if it doesn't exist or tenant changed, otherwise re-use potentially authenticated credential
        if (credential is not ChainedTokenCredential || tenantId != previousTenantId)
        {
            credential = new ChainedTokenCredential(sources.ToArray());
        }

        try
        {
            var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);
            return await GetTokenAsync(tokenRequestContext, cancellationToken);
        }
        catch (AuthenticationFailedException ex)
        {
            var errorMessage = "Could not get a token!";

            // Azure PowerShell serializes its errors to CLIXML
            // We parse using regex because the object itself is only an ANSI string from Get-AzAccessToken in the Az module
            var result = Regex.Match(ex.Message, @".+AAD\w+: (?<Message>.+\.)_x001B_");
            if (result.Success) {
                // If we managed to parse error, add it to message
                errorMessage += $" {result.Groups["Message"].Value}";
            }
            else
            {
                errorMessage += " See inner exception for more details.";
            }
            throw new AuthenticationFailedException(errorMessage, ex);
        }
    }

    /// <summary>
    /// Gets token noninteractively from existing named token cache.
    /// </summary>
    public static AzToken GetTokenFromCache(string resource, string[] scopes, string? claims, string? tenantId, string tokenCache, string? username, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenFromCacheAsync(resource, scopes, claims, tenantId, tokenCache, username, cancellationToken));

    /// <summary>
    /// Gets token noninteractively from existing named token cache.
    /// </summary>
    public static async Task<AzToken> GetTokenFromCacheAsync(
        string resource,
        string[] scopes,
        string? claims,
        string? tenantId,
        string tokenCache,
        string? username,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();

        if (string.IsNullOrWhiteSpace(tokenCache))
        {
            throw new ArgumentNullException(nameof(tokenCache), "The specified token cache cannot be null or empty!");
        }

        var options = new SharedTokenCacheCredentialOptions(new TokenCachePersistenceOptions { Name = tokenCache }) {
            Username = username
        };

        credential = new SharedTokenCacheCredential(options);

        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);
        return await GetTokenAsync(tokenRequestContext, cancellationToken);
    }

    /// <summary>
    /// Gets token interactively.
    /// </summary>
    public static AzToken GetTokenInteractive(string resource, string[] scopes, string? claims, string? clientId, string? tenantId, string tokenCache, int timeoutSeconds, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenInteractiveAsync(resource, scopes, claims, clientId, tenantId, tokenCache, timeoutSeconds, cancellationToken));

    /// <summary>
    /// Gets token interactively.
    /// </summary>
    public static async Task<AzToken> GetTokenInteractiveAsync(
        string resource,
        string[] scopes,
        string? claims,
        string? clientId,
        string? tenantId,
        string? tokenCache,
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        var options = new InteractiveBrowserCredentialOptions()
        {
            TokenCachePersistenceOptions = tokenCache is not null ? new TokenCachePersistenceOptions { Name = tokenCache } : null,
            ClientId = clientId
        };

        // Create a new credential
        credential = new InteractiveBrowserCredential(options);

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

    /// <summary>
    /// Gets token using a device code.
    /// </summary>
    public static async Task<AzToken> GetTokenDeviceCodeAsync(
        string resource,
        string[] scopes,
        string? claims,
        string? clientId,
        string? tenantId,
        string? tokenCache,
        int timeoutSeconds,
        BlockingCollection<string> loggingQueue,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        var options = new DeviceCodeCredentialOptions
        {
            // Register message in collection for Cmdlet to process
            DeviceCodeCallback = (deviceCodeInfo, cancellationToken) =>
                Task.Run(() => {
                    loggingQueue.Add(deviceCodeInfo.Message, cancellationToken);
                    // Make sure to mark as completed, no more messages can be sent
                    loggingQueue.CompleteAdding();
                }, cancellationToken),
            TokenCachePersistenceOptions = tokenCache is not null ? new TokenCachePersistenceOptions { Name = tokenCache } : null,
            ClientId = clientId
        };

        if (credential is not DeviceCodeCredential && previousClientId == clientId)
        {
            credential = new DeviceCodeCredential(options);
        }

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

    /// <summary>
    /// Gets token as a managed identity.
    /// </summary>
    public static AzToken GetTokenManagedIdentity(string resource, string[] scopes, string? claims, string? clientId, string? tenantId, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenManagedIdentityAsync(resource, scopes, claims, clientId, tenantId, cancellationToken));

    /// <summary>
    /// Gets token as a managed identity.
    /// </summary>
    public static async Task<AzToken> GetTokenManagedIdentityAsync(
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
        if (credential is not ManagedIdentityCredential && previousClientId == clientId)
        {
            credential = new ManagedIdentityCredential(clientId);
        }

        previousClientId = clientId;

        return await GetTokenAsync(tokenRequestContext, cancellationToken);
    }

    // Common method for getting the token from the credential depending on the user's authentication method.
    private static async Task<AzToken> GetTokenAsync(TokenRequestContext tokenRequestContext, CancellationToken cancellationToken)
    {
        if (credential is null)
        {
            throw new InvalidOperationException("Credential authorization could not be performed correctly, could not get token!");
        }
        previousTenantId = tokenRequestContext.TenantId;

        AccessToken token = await credential.GetTokenAsync(tokenRequestContext, cancellationToken);

        // Parse token to get info from claims
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token.Token);
        // Get upn of user if available, otherwise object id of identity used
        var identity = (jsonToken.Claims.FirstOrDefault(c => c.Type == "upn") ?? jsonToken.Claims.FirstOrDefault(c => c.Type == "oid"))?.Value;
        var tenantId = jsonToken.Claims.FirstOrDefault(c => c.Type == "tid");
        var scopes = jsonToken.Claims.FirstOrDefault(c => c.Type == "scp");

        return new AzToken(
            token.Token,
            scopes?.Value.Split(' ') ?? tokenRequestContext.Scopes,
            token.ExpiresOn,
            identity,
            tenantId?.Value ?? tokenRequestContext.TenantId
        );
    }

    public static void ClearCredential()
    {
        credential = null;
    }
}