using Azure.Core;
using Azure.Identity;
using System.Security.Cryptography.X509Certificates;

namespace PipeHow.AzAuth;

internal static partial class TokenManager
{
    /// <summary>
    /// Gets token with a client certificate.
    /// </summary>
    internal static AzToken GetTokenClientCertificate(string resource, string[] scopes, string? claims, string clientId, string tenantId, X509Certificate2 clientCertificate, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenClientCertificateAsync(resource, scopes, claims, clientId, tenantId, clientCertificate, cancellationToken));

    /// <summary>
    /// Gets token with a client certificate from a file path.
    /// </summary>
    internal static AzToken GetTokenClientCertificate(string resource, string[] scopes, string? claims, string clientId, string tenantId, string clientCertificatePath, CancellationToken cancellationToken) =>
        taskFactory.Run(() => GetTokenClientCertificateAsync(resource, scopes, claims, clientId, tenantId, clientCertificatePath, cancellationToken));

    /// <summary>
    /// Gets token with a client certificate.
    /// </summary>
    internal static async Task<AzToken> GetTokenClientCertificateAsync(
        string resource,
        string[] scopes,
        string? claims,
        string clientId,
        string tenantId,
        X509Certificate2 clientCertificate,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        credential = new ClientCertificateCredential(tenantId, clientId, clientCertificate);

        previousClientId = clientId;

        return await GetTokenAsync(tokenRequestContext, cancellationToken);
    }

    /// <summary>
    /// Gets token with a client certificate from a file path.
    /// </summary>
    internal static async Task<AzToken> GetTokenClientCertificateAsync(
        string resource,
        string[] scopes,
        string? claims,
        string clientId,
        string tenantId,
        string clientCertificatePath,
        CancellationToken cancellationToken)
    {
        var fullScopes = scopes.Select(s => $"{resource.TrimEnd('/')}/{s}").ToArray();
        var tokenRequestContext = new TokenRequestContext(fullScopes, null, claims, tenantId);

        credential = new ClientCertificateCredential(tenantId, clientId, clientCertificatePath);

        previousClientId = clientId;

        return await GetTokenAsync(tokenRequestContext, cancellationToken);
    }
}