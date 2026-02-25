using System.Management.Automation;

namespace PipeHow.AzAuth.Cmdlets;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
[Cmdlet(VerbsCommon.Get, "AzTokenCache")]
[OutputType(typeof(TokenCacheInfo[]))]
public class GetAzTokenCache : PSLoggerCmdletBase
{
    [Parameter]
    [Alias("IncludeAccounts")]
    public SwitchParameter IncludeDetails { get; set; }

    [Parameter]
    [ValidateNotNullOrEmpty]
    [ArgumentCompleter(typeof(ExistingCaches))]
    [Alias("Name")]
    public string? TokenCache { get; set; } = null;

    [Parameter]
    [ValidateNotNullOrEmpty]
    public string RootPath { get; set; } = CacheManager.GetCacheRootDirectory();

    protected override void EndProcessing()
    {
        try
        {
            WriteVerbose($"Scanning for token caches in the cache directory '{RootPath}'.");

            if (IncludeDetails.IsPresent)
            {
                WriteWarning("Including account information may prompt for access permissions, depending on the platform.");
            }

            var caches = CacheManager.GetAvailableCaches(TokenCache, RootPath, IncludeDetails.IsPresent, stopProcessing.Token);

            if (caches.Length == 0)
            {
                WriteVerbose("No token caches found.");
                return;
            }

            WriteObject(caches, true);
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(ex, "GetAzTokenCacheError", ErrorCategory.ReadError, null));
        }
    }
}
