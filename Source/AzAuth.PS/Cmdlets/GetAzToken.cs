using System.Collections.Concurrent;
using System.Management.Automation;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace PipeHow.AzAuth;

[Cmdlet(VerbsCommon.Get, "AzToken", DefaultParameterSetName = "NonInteractive")]
public class GetAzToken : PSLoggerCmdletBase
{
    [Parameter(ParameterSetName = "NonInteractive", Position = 0)]
    [Parameter(ParameterSetName = "Cache", Position = 0)]
    [Parameter(ParameterSetName = "Interactive", Position = 0)]
    [Parameter(ParameterSetName = "DeviceCode", Position = 0)]
    [Parameter(ParameterSetName = "ManagedIdentity", Position = 0)]
    [Parameter(ParameterSetName = "WorkloadIdentity", Position = 0)]
    [ValidateNotNullOrEmpty]
    [Alias("ResourceId", "ResourceUrl")]
    public string Resource { get; set; } = "https://graph.microsoft.com";

    [Parameter(ParameterSetName = "NonInteractive", Position = 1)]
    [Parameter(ParameterSetName = "Cache", Position = 1)]
    [Parameter(ParameterSetName = "Interactive", Position = 1)]
    [Parameter(ParameterSetName = "DeviceCode", Position = 1)]
    [Parameter(ParameterSetName = "ManagedIdentity", Position = 1)]
    [Parameter(ParameterSetName = "WorkloadIdentity", Position = 1)]
    [ValidateNotNullOrEmpty]
    public string[] Scope { get; set; } = new[] { ".default" };

    [Parameter(ParameterSetName = "NonInteractive")]
    [Parameter(ParameterSetName = "Cache")]
    [Parameter(ParameterSetName = "Interactive")]
    [Parameter(ParameterSetName = "DeviceCode")]
    [Parameter(ParameterSetName = "ManagedIdentity")]
    [Parameter(ParameterSetName = "WorkloadIdentity", Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public string TenantId { get; set; }

    [Parameter(ParameterSetName = "NonInteractive")]
    [Parameter(ParameterSetName = "Cache")]
    [Parameter(ParameterSetName = "Interactive")]
    [Parameter(ParameterSetName = "DeviceCode")]
    [Parameter(ParameterSetName = "ManagedIdentity")]
    [Parameter(ParameterSetName = "WorkloadIdentity")]
    [ValidateNotNullOrEmpty]
    public string Claim { get; set; }

    [Parameter(ParameterSetName = "Interactive")]
    [Parameter(ParameterSetName = "DeviceCode")]
    [Parameter(ParameterSetName = "ManagedIdentity")]
    [Parameter(ParameterSetName = "WorkloadIdentity", Mandatory = true)]
    [Parameter(ParameterSetName = "Cache")]
    [ValidateNotNullOrEmpty]
    public string ClientId { get; set; }

    [Parameter(ParameterSetName = "Interactive")]
    [Parameter(ParameterSetName = "DeviceCode")]
    [Parameter(ParameterSetName = "Cache", Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public string TokenCache { get; set; }

    [Parameter(ParameterSetName = "Cache", Mandatory = true)]
    [ValidateNotNullOrEmpty]
    [ArgumentCompleter(typeof(ExistingAccounts))]
    public string Username { get; set; }

    [Parameter(ParameterSetName = "Interactive")]
    [Parameter(ParameterSetName = "DeviceCode")]
    [ValidateRange(1, int.MaxValue)]
    public int TimeoutSeconds { get; set; } = 120;

    [Parameter(ParameterSetName = "Interactive", Mandatory = true)]
    public SwitchParameter Interactive { get; set; }

    [Parameter(ParameterSetName = "DeviceCode", Mandatory = true)]
    public SwitchParameter DeviceCode { get; set; }

    [Parameter(ParameterSetName = "ManagedIdentity", Mandatory = true)]
    public SwitchParameter ManagedIdentity { get; set; }

    [Parameter(ParameterSetName = "WorkloadIdentity", Mandatory = true)]
    public SwitchParameter WorkloadIdentity { get; set; }

    [Parameter(ParameterSetName = "WorkloadIdentity", Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public string ExternalToken { get; set; }

    [Parameter(ParameterSetName = "NonInteractive")]
    [Parameter(ParameterSetName = "Interactive")]
    [Parameter(ParameterSetName = "DeviceCode")]
    [Parameter(ParameterSetName = "ManagedIdentity")]
    [Parameter(ParameterSetName = "WorkloadIdentity")]
    public SwitchParameter Force { get; set; }

    // If user specifies Force, disregard earlier authentication
    protected override void BeginProcessing()
    {
        base.BeginProcessing();

        if (Force.IsPresent)
        {
            TokenManager.ClearCredential();
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD104:Offer async methods", Justification = "PowerShell doesn't handle async.")]
    protected override void EndProcessing()
    {
        if (TokenCache == "msal.cache")
        {
            WriteWarning("The name 'msal.cache' is the default cache name in MSAL, changing or clearing this cache might break other tools using it!");
        }

        WriteVerbose($"Getting token for {Resource} with scopes: {string.Join(", ", Scope)}.");

        if (ParameterSetName == "NonInteractive")
        {
            WriteVerbose(@"Looking for a token from the following sources:
Environment variables (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.environmentcredential)
Azure PowerShell (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.azurepowershellcredential)
Azure CLI (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.azureclicredential)
Visual Studio Code (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.visualstudiocodecredential)
Visual Studio (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.visualstudiocredential)
Shared token cache (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.sharedtokencachecredential)
");
            WriteObject(TokenManager.GetTokenNonInteractive(Resource, Scope, Claim, TenantId, stopProcessing.Token));
        }
        else if (ParameterSetName == "Cache")
        {
            WriteVerbose($"Getting token from token cache named \"{TokenCache}\".");
            WriteObject(TokenManager.GetTokenFromCache(Resource, Scope, Claim, ClientId, TenantId, TokenCache!, Username, stopProcessing.Token));
        }
        else if (Interactive.IsPresent)
        {
            WriteVerbose("Getting token interactively using the default browser.");
            WriteObject(TokenManager.GetTokenInteractive(Resource, Scope, Claim, ClientId, TenantId, TokenCache, TimeoutSeconds, stopProcessing.Token));
        }
        else if (DeviceCode.IsPresent)
        {
            WriteVerbose("Getting token using the device code flow.");

            // Set up a BlockingCollection to use for logging device code message
            BlockingCollection<string> loggingQueue = new();
            // Start device code flow and save task
            var tokenTask = joinableTaskFactory.RunAsync(() => TokenManager.GetTokenDeviceCodeAsync(Resource, Scope, Claim, ClientId, TenantId, TokenCache, TimeoutSeconds, loggingQueue, stopProcessing.Token));

            // Loop through messages and log them to warning stream (verbose is silent by default)
            try
            {
                while (loggingQueue.TryTake(out string? message, Timeout.Infinite, stopProcessing.Token))
                {
                    WriteWarning(message);
                }
            }
            catch (OperationCanceledException) { /* It's fine if user cancels */ }

            // The device code message has been presented, now await task and output token when done
            WriteObject(tokenTask.Join(stopProcessing.Token));
        }
        else if (ManagedIdentity.IsPresent)
        {
            WriteVerbose("Getting token using a managed identity (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.managedidentitycredential).");
            WriteObject(TokenManager.GetTokenManagedIdentity(Resource, Scope, Claim, ClientId, TenantId, stopProcessing.Token));
        }
        else if (WorkloadIdentity.IsPresent)
        {
            WriteVerbose($"Getting token using workload identity federation (using client assertion) for client \"{ClientId}\" (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.clientassertioncredential).");
            WriteObject(TokenManager.GetTokenWorkloadIdentity(Resource, Scope, Claim, ClientId, TenantId, ExternalToken, stopProcessing.Token));
        }
        else
        {
            throw new ArgumentException("Invalid parameter combination!");
        }
    }
}