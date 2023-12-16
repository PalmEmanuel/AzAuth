using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Threading;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;

namespace PipeHow.AzAuth;

internal static class TokenManager
{
    private static TokenCredential? credential;
    private static string? previousTenantId;
    private static string? previousClientId;
    private static readonly JoinableTaskFactory taskFactory = new(new JoinableTaskContext());


    /// <summary>
    /// Gets token noninteractively.
    /// </summary>
    internal static AzToken GetTokenNonInteractive(string resource, string[] scopes, string? claims, string? tenantId, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenNonInteractiveAsync(resource, scopes, claims, tenantId, cancellationToken));

    /// <summary>
    /// Gets token noninteractively.
    /// </summary>
    internal static async Task<AzToken> GetTokenNonInteractiveAsync(
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
    internal static AzToken GetTokenFromCache(string resource, string[] scopes, string? claims, string? clientId, string? tenantId, string tokenCache, string username, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenFromCacheAsync(resource, scopes, claims, clientId, tenantId, tokenCache, username, cancellationToken));

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
            return await CacheManager.GetTokenFromCacheSilentAsync(tokenCache, clientId, tenantId, fullScopes, claims, username, cancellationToken);
        }
        catch (MsalServiceException ex) when (ex.Message.Contains("Please do not use the /consumers endpoint to serve this request."))
        {
            throw new InvalidOperationException("The account used is a personal account and cannot use a general endpoint. Please specify the tenant using the parameter -TenantId.", ex);
        }
    }

    /// <summary>
    /// Gets token interactively.
    /// </summary>
    internal static AzToken GetTokenInteractive(string resource, string[] scopes, string? claims, string? clientId, string? tenantId, string? tokenCache, int timeoutSeconds, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenInteractiveAsync(resource, scopes, claims, clientId, tenantId, tokenCache, timeoutSeconds, cancellationToken));

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
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        if (!string.IsNullOrWhiteSpace(tokenCache))
        {
            return CacheManager.GetTokenInteractive(tokenCache, clientId, tenantId, fullScopes, claims, cancellationToken);
        }

        var options = new InteractiveBrowserCredentialOptions
        {
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
    internal static async Task<AzToken> GetTokenDeviceCodeAsync(
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

        // If user specified a cache name, get token using cache manager
        if (tokenCache != null)
        {
            // Create a new cancellation token by combining a timeout with existing token
            using var timeoutSource = new CancellationTokenSource(timeoutSeconds * 1000);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token).Token;
            return await CacheManager.GetTokenDeviceCodeAsync(tokenCache, clientId, tenantId, fullScopes, claims, loggingQueue, combinedToken);
        }

        var options = new DeviceCodeCredentialOptions
        {
            // Register message in collection for Cmdlet to process
            DeviceCodeCallback = (deviceCodeInfo, cancellationToken) =>
                Task.Run(() => {
                    loggingQueue.Add(deviceCodeInfo.Message, cancellationToken);
                    // Make sure to mark as completed, no more messages can be sent
                    loggingQueue.CompleteAdding();
                }, cancellationToken),
            ClientId = clientId
        };

        if (credential is not DeviceCodeCredential || previousClientId != clientId)
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

    /// <summary>
    /// Gets token with a client secret.
    /// </summary>
    internal static AzToken GetTokenClientSecret(string resource, string[] scopes, string? claims, string? clientId, string tenantId, string clientSecret, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenClientSecretAsync(resource, scopes, claims, clientId, tenantId, clientSecret, cancellationToken));

    /// <summary>
    /// Gets token with a client secret.
    /// </summary>
    internal static async Task<AzToken> GetTokenClientSecretAsync(
        string resource,
        string[] scopes,
        string? claims,
        string? clientId,
        string tenantId,
        string clientSecret,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        // Re-use the previous credential if client id didn't change
        if (credential is not ClientSecretCredential || previousClientId != clientId)
        {
            credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        }

        previousClientId = clientId;

        return await GetTokenAsync(tokenRequestContext, cancellationToken);
    }




    // Common method for getting the token from the credential depending on the user's authentication method.
    internal static async Task<AzToken> GetTokenAsync(TokenRequestContext tokenRequestContext, CancellationToken cancellationToken)
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
        // Get upn of user if available, or email if personal account, otherwise object id of identity used (such as managed identity)
        var identity = (jsonToken.Claims.FirstOrDefault(c => c.Type == "upn") ??
            jsonToken.Claims.FirstOrDefault(c => c.Type == "email") ??
            jsonToken.Claims.FirstOrDefault(c => c.Type == "oid"))?.Value;
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

    internal static void ClearCredential()
    {
        credential = null;
    }
}