using Azure.Core;
using Azure.Identity;
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Threading.Tasks;

namespace PipeHow.AzAuth;

internal static partial class TokenManager
{
    /// <summary>
    /// Gets token using a device code.
    /// </summary>
    internal static AzToken GetTokenDeviceCode(
        string resource,
        string[] scopes,
        string? claims,
        string? clientId,
        string? tenantId,
        string? tokenCache,
        int timeoutSeconds,
        PSCmdlet cmdlet,
        CancellationToken cancellationToken)
    {
        // Set up a BlockingCollection to use for logging device code message
        BlockingCollection<string> loggingQueue = new();

        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        // If user specified a cache name, get token using cache manager
        if (tokenCache != null)
        {
            // Create a new cancellation token by combining a timeout with existing token
            using var timeoutSource = new CancellationTokenSource(timeoutSeconds * 1000);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token).Token;
            return taskFactory.Run(() => CacheManager.GetTokenDeviceCodeAsync(tokenCache, clientId, tenantId, fullScopes, claims, loggingQueue, combinedToken));
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
            var tokenTask = taskFactory.RunAsync(() => GetTokenAsync(tokenRequestContext, combinedToken));

            // Loop through messages and log them to warning stream (verbose is silent by default)
            try
            {
                while (loggingQueue.TryTake(out string? message, Timeout.Infinite, cancellationToken))
                {
                    cmdlet.WriteWarning(message);
                }
            }
            catch (OperationCanceledException) { /* It's fine if user cancels here, no need to write error message */ }

            return tokenTask.Join(cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            // Only the timeout is caught here, CTRL + C in PowerShell does not throw an error
            throw new OperationCanceledException($"Login timed out after {timeoutSeconds} seconds, configured by the TimeoutSeconds parameter.", ex);
        }
    }
}