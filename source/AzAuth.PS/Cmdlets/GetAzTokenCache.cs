using System.Management.Automation;

namespace PipeHow.AzAuth.Cmdlets;

[Cmdlet(VerbsCommon.Get, "AzTokenCache")]
[OutputType(typeof(TokenCacheInfo[]))]
public class GetAzTokenCache : PSLoggerCmdletBase
{
    [Parameter]
    public SwitchParameter IncludeAccounts { get; set; }

    [Parameter]
    [ValidateNotNullOrEmpty]
    public string RootPath { get; set; } = CacheManager.GetCacheRootDirectory();

    protected override void EndProcessing()
    {
        try
        {
            WriteVerbose($"Scanning for token caches in the cache directory '{RootPath}'.");

            if (IncludeAccounts.IsPresent)
            {
                WriteWarning("Including account information may prompt for access depending on the platform.");
            }

            var caches = CacheManager.GetAvailableCaches(RootPath, IncludeAccounts.IsPresent, stopProcessing.Token);

            if (caches.Length == 0)
            {
                WriteVerbose("No token caches found.");
                return;
            }

            WriteVerbose($"Found {caches.Count()} token cache(s).");

            WriteObject(caches, true);
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(ex, "GetAzTokenCacheError", ErrorCategory.ReadError, null));
        }
    }
}
