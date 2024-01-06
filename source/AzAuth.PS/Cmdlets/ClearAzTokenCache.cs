using System.Management.Automation;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace PipeHow.AzAuth.Cmdlets;

[Cmdlet(VerbsCommon.Clear, "AzTokenCache")]
public class ClearAzTokenCache : PSLoggerCmdletBase
{
    [Parameter(Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public string TokenCache { get; set; }

    protected override void EndProcessing()
    {
        if (TokenCache == "msal.cache")
        {
            WriteWarning("The name 'msal.cache' is the default cache name in MSAL, changing or clearing this cache might break other tools using it!");
        }
        CacheManager.ClearCache(TokenCache, stopProcessing.Token);
    }
}