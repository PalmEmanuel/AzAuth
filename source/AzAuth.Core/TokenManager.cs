using Azure.Core;
using Microsoft.VisualStudio.Threading;
using System.IdentityModel.Tokens.Jwt;

namespace PipeHow.AzAuth;

internal static partial class TokenManager
{
    private static TokenCredential? credential;
    private static string? previousTenantId;
    private static string? previousClientId;
    private static readonly JoinableTaskFactory taskFactory = new(new JoinableTaskContext());

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

        var claims = new ClaimsDictionary();
        foreach (var claim in jsonToken.Claims)
        {
            claims.Add(claim.Type, claim.Value);
        }

        return new AzToken(
            token.Token,
            scopes?.Value.Split(' ') ?? tokenRequestContext.Scopes,
            token.ExpiresOn,
            claims,
            identity,
            tenantId?.Value ?? tokenRequestContext.TenantId
        );
    }

    internal static void ClearCredential()
    {
        credential = null;
    }

    internal static bool HasClientId()
    {
        return !string.IsNullOrEmpty(previousClientId);
    }
}