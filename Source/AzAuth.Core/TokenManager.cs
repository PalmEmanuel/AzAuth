using AzAuth.Core.Models;
using Azure.Core;
using Azure.Identity;
using System.Threading;

namespace AzAuth.Core;

public static class TokenManager
{
    private static AuthenticationRecord? authenticationRecord;
    private static TokenCredential? credential;

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

        // Create a new credential if it doesn't exist, otherwise re-use potentially authenticated credential
        // Create our own credential chain because we want to change the order
        credential ??= new ChainedTokenCredential(
            new EnvironmentCredential(),
            new SharedTokenCacheCredential(),
            new AzurePowerShellCredential(),
            new AzureCliCredential(),
            new VisualStudioCodeCredential(),
            new VisualStudioCredential()
        );

        var tokenRequestContext = new TokenRequestContext(fullScopes, claims: claims, tenantId: tenantId);
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
        var tokenRequestContext = new TokenRequestContext(fullScopes, claims: claims, tenantId: tenantId);

        // If credential does not exist, is null, or clientid is not same as previously
        if (credential is not InteractiveBrowserCredential || authenticationRecord?.ClientId != clientId)
        {
            // Set clientid if provided
            var options = new InteractiveBrowserCredentialOptions();
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                options.ClientId = clientId;
            }

            // Create a new credential
            credential = new InteractiveBrowserCredential(options);
            // Authenticate first and save the authentication info to compare client id of future requests
            authenticationRecord = ((InteractiveBrowserCredential)credential).Authenticate(tokenRequestContext, cancellationToken);
        }

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
        var tokenRequestContext = new TokenRequestContext(fullScopes, claims: claims, tenantId: tenantId);

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

        AccessToken token = credential.GetToken(tokenRequestContext, cancellationToken);
        return new AzToken(token.Token, tokenRequestContext.Scopes, token.ExpiresOn);
    }
}