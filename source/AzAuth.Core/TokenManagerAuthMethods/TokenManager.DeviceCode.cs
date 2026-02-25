using Azure.Core;
using Azure.Identity;
using System.Collections.Concurrent;

namespace PipeHow.AzAuth;

internal static partial class TokenManager
{
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
        string rootDir,
        int timeoutSeconds,
        bool useUnprotectedTokenCache,
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
            return await CacheManager.GetTokenDeviceCodeAsync(tokenCache, rootDir, clientId, tenantId, fullScopes, claims, useUnprotectedTokenCache, loggingQueue, combinedToken);
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
}