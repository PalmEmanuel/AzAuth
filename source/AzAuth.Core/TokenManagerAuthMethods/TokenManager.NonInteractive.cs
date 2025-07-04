﻿using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Identity;
using System.Text.RegularExpressions;

namespace PipeHow.AzAuth;

internal static partial class TokenManager
{
    /// <summary>
    /// Gets token noninteractively.
    /// </summary>
    internal static AzToken GetTokenNonInteractive(string resource, string[] scopes, string? claims, string? tenantId, string[] credentialPrecedence, int? timeoutSeconds, int managedIdentityTimeoutSeconds, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenNonInteractiveAsync(resource, scopes, claims, tenantId, credentialPrecedence, timeoutSeconds, managedIdentityTimeoutSeconds, cancellationToken));

    /// <summary>
    /// Gets token noninteractively.
    /// </summary>
    internal static async Task<AzToken> GetTokenNonInteractiveAsync(
        string resource,
        string[] scopes,
        string? claims,
        string? tenantId,
        string[] credentialPrecedence,
        int? timeoutSeconds,
        int managedIdentityTimeoutSeconds,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();

        // If timeoutSeconds is not null, create a new TokenCredentialOptions with the specified timeout
        // Otherwise, set to null to use default timeout
        TokenCredentialOptions? genericTimeoutOptions = timeoutSeconds.HasValue ? new TokenCredentialOptions
        {
            Retry = {
                NetworkTimeout = TimeSpan.FromSeconds(timeoutSeconds.Value),
                MaxRetries = 0,
                Delay = TimeSpan.Zero,
                MaxDelay = TimeSpan.Zero
            }
        } : null;

        // Create our own credential chain based on user parameter to change the order
        var sources = new List<TokenCredential>();
        foreach (var credentialType in credentialPrecedence)
        {
            sources.Add(credentialType switch
            {
                "ManagedIdentity" => new ManagedIdentityCredential(options: new ManagedIdentityCredentialOptions
                {
                    Retry = {
                        NetworkTimeout = TimeSpan.FromSeconds(managedIdentityTimeoutSeconds),
                        MaxRetries = 0,
                        Delay = TimeSpan.Zero,
                        MaxDelay = TimeSpan.Zero
                    }
                }),
                "Environment" => new EnvironmentCredential(genericTimeoutOptions),
                "AzurePowerShell" => new AzurePowerShellCredential(genericTimeoutOptions as AzurePowerShellCredentialOptions),
                "AzureCLI" => new AzureCliCredential(genericTimeoutOptions as AzureCliCredentialOptions),
                "VisualStudio" => new VisualStudioCredential(genericTimeoutOptions as VisualStudioCredentialOptions),
                "SharedTokenCache" => new SharedTokenCacheCredential(genericTimeoutOptions as SharedTokenCacheCredentialOptions),
                _ => throw new ArgumentException("Invalid credential type", nameof(credentialType))
            });
        }

        bool sameTenant = tenantId == previousTenantId;
        bool sameCredentialPrecedence = previousCredentialPrecedence is not null && credentialPrecedence.SequenceEqual(previousCredentialPrecedence);

        // If user authenticated interactively in the same session and tenant didn't change, add it as the first option to find tokens from
        if (credential is InteractiveBrowserCredential && sameTenant && sameCredentialPrecedence)
        {
            sources.Insert(0, credential);
        }

        // Create a new credential if it doesn't exist, or tenant or credential precedence changed, otherwise re-use potentially authenticated credential
        if (credential is not ChainedTokenCredential || !sameTenant || !sameCredentialPrecedence)
        {
            credential = new ChainedTokenCredential(sources.ToArray());
            previousCredentialPrecedence = credentialPrecedence;
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
                errorMessage += " See inner exception for more details: " + Environment.NewLine + ex.Message;
            }
            throw new AuthenticationFailedException(errorMessage, ex);
        }
    }
}