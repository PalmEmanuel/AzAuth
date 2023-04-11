namespace PipeHow.AzAuth;

public class AzToken
{
    public string Token { get; set; }
    public DateTimeOffset ExpiresOn { get; set; }
    public string[] Scopes { get; set; }

    public AzToken(string token, string[] scopes, DateTimeOffset expiresOn)
    {
        Token = token;
        Scopes = scopes;
        ExpiresOn = expiresOn;
    }

    public override string ToString()
    {
        return Token;
    }
}
