namespace PipeHow.AzAuth;

public class TokenCacheInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime LastModified { get; set; }
    public TokenCacheProtection Protection { get; set; } = TokenCacheProtection.Unknown;
    public bool CacheDetailsChecked { get; set; } = false;
    public int? AccountCount { get; set; } = null;
    public string[]? Accounts { get; set; } = null;
}

public enum TokenCacheProtection
{
    Unprotected,
    Protected,
    Unknown
}