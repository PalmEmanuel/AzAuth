using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace PipeHow.AzAuth;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Usage",
    "CA2254:Template should be a static expression",
    Justification = "Logging to PowerShell streams.")]
public static class TokenManager
{
    private static TokenCredential? credential;
    private static string? previousTenantId;

    internal static ILogger? Logger { get; set; }

    /// <summary>
    /// Gets token noninteractively.
    /// </summary>
    public static AzToken GetTokenNonInteractive(
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

        // If the user has authenticated interactively in the same session, to the same tenant, add it as the first option to find tokens from
        if (credential is InteractiveBrowserCredential && tenantId == previousTenantId)
        {
            sources.Insert(0, credential);
        }

        // Create a new credential if it doesn't exist or tenant changed, otherwise re-use potentially authenticated credential
        if (credential is not ChainedTokenCredential || tenantId != previousTenantId)
        {
            credential = new ChainedTokenCredential(sources.ToArray());
        }

        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);
        return GetToken(tokenRequestContext, cancellationToken);
    }

    /// <summary>
    /// Gets token noninteractively from existing named token cache.
    /// </summary>
    public static AzToken GetTokenFromCache(
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

        var options = new SharedTokenCacheCredentialOptions(
            new TokenCachePersistenceOptions { Name = tokenCache }
        );

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            options.TenantId = tenantId;
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            options.Username = username;
        }

        credential = new SharedTokenCacheCredential(options);

        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);
        return GetToken(tokenRequestContext, cancellationToken);
    }

    /// <summary>
    /// Gets token interactively.
    /// </summary>
    public static AzToken GetTokenInteractive(
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

        var options = new InteractiveBrowserCredentialOptions();

        // Serialize to named token cache if provided
        if (!string.IsNullOrWhiteSpace(tokenCache)) {
            options.TokenCachePersistenceOptions = new TokenCachePersistenceOptions { Name = tokenCache };
        }

        // Set tenant id if provided
        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            options.TenantId = tenantId;
        }

        // Set client id if provided
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            options.ClientId = clientId;
        }

        // Create a new credential
        credential = new InteractiveBrowserCredential(options);

        try
        {
            // Create a new cancellation token by combining a timeout with existing token
            using var timeoutSource = new CancellationTokenSource(timeoutSeconds * 1000);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token).Token;
            return GetToken(tokenRequestContext, combinedToken);
        }
        catch (OperationCanceledException ex)
        {
            // Only the timeout is caught here, CTRL + C in PowerShell does not throw an error
            throw new OperationCanceledException($"Login timed out after {timeoutSeconds} seconds, configured by the TimeoutSeconds parameter.", ex);
        }
    }

    public static AzToken GetTokenDeviceCode(
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

        var options = new DeviceCodeCredentialOptions
        {
            // Make sure to log the message
            DeviceCodeCallback = (deviceCodeInfo, cancellationToken) =>
            {
                Logger?.LogInformation(deviceCodeInfo.Message);
                return Task.CompletedTask;
            }
        };

        // Set tenant id if provided
        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            options.TenantId = tenantId;
        }

        // Serialize to named token cache if provided
        if (!string.IsNullOrWhiteSpace(tokenCache)) {
            options.TokenCachePersistenceOptions = new TokenCachePersistenceOptions { Name = tokenCache };
        }

        // Set client id if provided
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            options.ClientId = clientId;
        }

        if (credential is not DeviceCodeCredential)
        {
            credential = new DeviceCodeCredential(options);
        }

        try
        {
            // Create a new cancellation token by combining a timeout with existing token
            using var timeoutSource = new CancellationTokenSource(timeoutSeconds * 1000);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token).Token;
            return GetToken(tokenRequestContext, combinedToken);
        }
        catch (OperationCanceledException ex)
        {
            // Only the timeout is caught here, CTRL + C in PowerShell does not throw an error
            throw new OperationCanceledException($"Login timed out after {timeoutSeconds} seconds, configured by the TimeoutSeconds parameter.", ex);
        }
    }

    /// <summary>
    /// Gets token as managed identity.
    /// </summary>
    public static AzToken GetTokenManagedIdentity(
        string resource,
        string[] scopes,
        string? claims,
        string? clientId,
        string? tenantId,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        if (credential is not ManagedIdentityCredential)
        {
            credential = new ManagedIdentityCredential(clientId);
        }

        return GetToken(tokenRequestContext, cancellationToken);
    }

    // Common method for getting the token from the credential depending on the user's authentication method.
    private static AzToken GetToken(TokenRequestContext tokenRequestContext, CancellationToken cancellationToken)
    {
        if (credential is null)
        {
            throw new InvalidOperationException("Credential authorization could not be performed correctly, could not get token!");
        }
        previousTenantId = tokenRequestContext.TenantId;

        AccessToken token = credential.GetToken(tokenRequestContext, cancellationToken);

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