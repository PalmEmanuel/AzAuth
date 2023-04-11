using AzAuth.Core.Models;
using Azure.Core;
using Azure.Identity;
using System.Threading;

namespace AzAuth.Core;

public static class TokenManager
{
    private static TokenCredential? credential;
    private static string? previousTenantId;

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
            new VisualStudioCredential()
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
    /// Gets token interactively.
    /// </summary>
    public static AzToken GetTokenInteractive(
        string resource,
        string[] scopes,
        string? claims,
        string? clientId,
        string? tenantId,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        // Set clientid if provided
        var options = new InteractiveBrowserCredentialOptions();
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            options.ClientId = clientId;
        }

        // Create a new credential
        credential = new InteractiveBrowserCredential(options);

        return GetToken(tokenRequestContext, cancellationToken);
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
            throw new InvalidOperationException("Credential was null when trying to get token!");
        }
        previousTenantId = tokenRequestContext.TenantId;

        AccessToken token = credential.GetToken(tokenRequestContext, cancellationToken);
        return new AzToken(token.Token, tokenRequestContext.Scopes, token.ExpiresOn);
    }

    public static void ClearCredential()
    {
        credential = null;
    }
}