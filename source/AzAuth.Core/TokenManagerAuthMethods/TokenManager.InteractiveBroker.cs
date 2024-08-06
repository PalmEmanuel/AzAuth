using Azure.Core;
using Azure.Identity;
using Azure.Identity.Broker;
using System.Runtime.InteropServices;

namespace PipeHow.AzAuth;

internal static partial class TokenManager
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetForegroundWindow();

    internal static IntPtr GetForegroundWindowHandle() =>
        GetForegroundWindow();

    /// <summary>
    /// Gets token interactively using the Web Account Manager Broker.
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="scopes"></param>
    /// <param name="claims"></param>
    /// <param name="clientId"></param>
    /// <param name="tenantId"></param>
    /// <param name="tokenCache"></param>
    /// <param name="timeoutSeconds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal static AzToken GetTokenInteractiveBroker(string resource, string[] scopes, string? claims, string? clientId, string? tenantId, string? tokenCache, int timeoutSeconds, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenInteractiveBrokerAsync(resource, scopes, claims, clientId, tenantId, tokenCache, timeoutSeconds, cancellationToken));

    /// <summary>
    /// Gets token interactively using the Web Account Manager Broker.
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="scopes"></param>
    /// <param name="claims"></param>
    /// <param name="clientId"></param>
    /// <param name="tenantId"></param>
    /// <param name="tokenCache"></param>
    /// <param name="timeoutSeconds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException"></exception>
    internal static async Task<AzToken> GetTokenInteractiveBrokerAsync(
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
            return await CacheManager.GetTokenInteractiveAsync(tokenCache!, clientId, tenantId, fullScopes, claims, cancellationToken);

        IntPtr parentWindow = GetForegroundWindowHandle();
        var options = new InteractiveBrowserCredentialBrokerOptions(parentWindow);

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
        
}
