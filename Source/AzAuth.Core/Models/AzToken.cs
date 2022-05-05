namespace AzAuth.Core.Models
{
    public class AzToken
    {
        public string Token { get; set; }
        public DateTimeOffset ExpiresOn { get; set; }
        public string ResourceUrl { get; set; }
        public string[] Scopes { get; set; }

        public AzToken(string token, string resourceUrl, string[] scopes, DateTimeOffset expiresOn)
        {
            Token = token;
            ResourceUrl = resourceUrl;
            Scopes = scopes;
            ExpiresOn = expiresOn;
        }

        public override string ToString()
        {
            return Token;
        }
    }
}
