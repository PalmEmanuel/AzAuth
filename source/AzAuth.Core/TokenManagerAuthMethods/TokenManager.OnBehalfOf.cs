using Azure.Core;
using Azure.Identity;
using System.Security.Cryptography.X509Certificates;

namespace PipeHow.AzAuth;

internal static partial class TokenManager
{
    /// <summary>
    /// Gets token using the on-behalf-of flow with a client secret.
    /// </summary>
    internal static AzToken GetTokenOnBehalfOf(string resource, string[] scopes, string? claims, string clientId, string tenantId, string clientSecret, string userAssertion, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenOnBehalfOfAsync(resource, scopes, claims, clientId, tenantId, clientSecret, userAssertion, cancellationToken));

    /// <summary>
    /// Gets token using the on-behalf-of flow with a client certificate.
    /// </summary>
    internal static AzToken GetTokenOnBehalfOfCertificate(string resource, string[] scopes, string? claims, string clientId, string tenantId, X509Certificate2 clientCertificate, string userAssertion, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenOnBehalfOfCertificateAsync(resource, scopes, claims, clientId, tenantId, clientCertificate, userAssertion, cancellationToken));

    /// <summary>
    /// Gets token using the on-behalf-of flow with a client certificate from a file path.
    /// </summary>
    internal static AzToken GetTokenOnBehalfOfCertificatePath(string resource, string[] scopes, string? claims, string clientId, string tenantId, string clientCertificatePath, string userAssertion, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenOnBehalfOfCertificatePathAsync(resource, scopes, claims, clientId, tenantId, clientCertificatePath, userAssertion, cancellationToken));

    /// <summary>
    /// Gets token using the on-behalf-of flow with a client secret.
    /// </summary>
    internal static async Task<AzToken> GetTokenOnBehalfOfAsync(
        string resource,
        string[] scopes,
        string? claims,
        string clientId,
        string tenantId,
        string clientSecret,
        string userAssertion,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        credential = new OnBehalfOfCredential(tenantId, clientId, clientSecret, userAssertion);
        previousClientId = clientId;

        return await GetTokenAsync(tokenRequestContext, cancellationToken);
    }

    /// <summary>
    /// Gets token using the on-behalf-of flow with a client certificate.
    /// </summary>
    internal static async Task<AzToken> GetTokenOnBehalfOfCertificateAsync(
        string resource,
        string[] scopes,
        string? claims,
        string clientId,
        string tenantId,
        X509Certificate2 clientCertificate,
        string userAssertion,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        credential = new OnBehalfOfCredential(tenantId, clientId, clientCertificate, userAssertion);
        previousClientId = clientId;

        return await GetTokenAsync(tokenRequestContext, cancellationToken);
    }

    /// <summary>
    /// Gets token using the on-behalf-of flow with a client certificate from a file path.
    /// </summary>
    internal static async Task<AzToken> GetTokenOnBehalfOfCertificatePathAsync(
        string resource,
        string[] scopes,
        string? claims,
        string clientId,
        string tenantId,
        string clientCertificatePath,
        string userAssertion,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        var cert = new X509Certificate2(clientCertificatePath);
        credential = new OnBehalfOfCredential(tenantId, clientId, cert, userAssertion);
        previousClientId = clientId;

        return await GetTokenAsync(tokenRequestContext, cancellationToken);
    }
}
