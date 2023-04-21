namespace PipeHow.AzAuth;

public class AzToken
{
    public string Token { get; }
    public DateTimeOffset ExpiresOn { get; }
    public string? Identity { get; }
    public string? TenantId { get; }
    public string[] Scopes { get; }

    public AzToken(string token, string[] scopes, DateTimeOffset expiresOn, string? identity = null, string? tenantId = null)
    {
        Token = token;
        Identity = identity;
        TenantId = tenantId;
        Scopes = scopes;
        ExpiresOn = expiresOn;
    }

    public override string ToString()
    {
        return Token;
    }
}
