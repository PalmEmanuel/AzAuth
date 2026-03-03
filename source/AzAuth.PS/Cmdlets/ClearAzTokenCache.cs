using System.Management.Automation;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace PipeHow.AzAuth.Cmdlets;

[Cmdlet(VerbsCommon.Clear, "AzTokenCache")]
public class ClearAzTokenCache : PSLoggerCmdletBase
{
    [Parameter(Mandatory = true, Position = 0)]
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
            try
            {
                CacheManager.RemoveCache(TokenCache, RootPath, stopProcessing.Token);
            }
            catch (DirectoryNotFoundException) {
                // If the cache (it's a directory) doesn't exist, it's likely cleared but we output verbose info for the user
                WriteWarning($"The token cache '{TokenCache}' does not exist.");
            }
        }
        else
        {
            try
            {   // Assume the cache is protected
                WriteVerbose($"Trying to clear token cache '{TokenCache}' as protected.");
                CacheManager.ClearCache(TokenCache, RootPath, false, stopProcessing.Token);
            }
            catch
            {
                try
                {   // Otherwise try to clear it as unprotected
                    WriteVerbose($"Trying to clear token cache '{TokenCache}' as unprotected.");
                    CacheManager.ClearCache(TokenCache, RootPath, true, stopProcessing.Token);
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(new Exception($"Failed to clear the token cache '{TokenCache}'. Consider using -Force to remove the cache files directly, but be aware this may cause issues with other applications using the same cache.", ex), "ClearAzTokenCacheError", ErrorCategory.WriteError, null));
                }
            }
        }
    }
}