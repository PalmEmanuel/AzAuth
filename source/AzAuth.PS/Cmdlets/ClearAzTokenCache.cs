using System.Management.Automation;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace PipeHow.AzAuth.Cmdlets;

[Cmdlet(VerbsCommon.Clear, "AzTokenCache")]
public class ClearAzTokenCache : PSLoggerCmdletBase
{
    [Parameter(Mandatory = true)]
    [ValidateNotNullOrEmpty]
    [ArgumentCompleter(typeof(ExistingCaches))]
    [Alias("Name")]
    public string TokenCache { get; set; }

    [Parameter]
    public SwitchParameter Force { get; set; }

    [Parameter]
    [ValidateNotNullOrEmpty]
    public string RootPath { get; set; } = CacheManager.GetCacheRootDirectory();

    protected override void EndProcessing()
    {
        if (TokenCache == "msal.cache")
        {
            WriteWarning("The name 'msal.cache' is the default cache name in MSAL, changing or clearing this cache might break other tools using it!");
        }

        if (Force)
        {
            WriteWarning($"Will attempt to delete all files for the token cache '{TokenCache}', this may cause issues with other applications using the same cache!");
            CacheManager.RemoveCache(TokenCache, RootPath, stopProcessing.Token);
        }
        else
        {
            CacheManager.ClearCache(TokenCache, RootPath, false, stopProcessing.Token);
            CacheManager.ClearCache(TokenCache, RootPath, true, stopProcessing.Token);
        }
    }
}