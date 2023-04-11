using AzAuth.Core;
using System.Management.Automation;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace AzAuth.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "AzToken", DefaultParameterSetName = "NonInteractive")]
    public class GetAzToken : PSCmdlet
    {
        [Parameter(ParameterSetName = "NonInteractive")]
        [Parameter(ParameterSetName = "Interactive")]
        [Parameter(ParameterSetName = "ManagedIdentity")]
        [ValidateNotNullOrEmpty()]
        [Alias("ResourceId", "ResourceUrl")]
        public string Resource { get; set; } = "https://graph.microsoft.com";

        [Parameter(ParameterSetName = "NonInteractive")]
        [Parameter(ParameterSetName = "Interactive")]
        [Parameter(ParameterSetName = "ManagedIdentity")]
        [ValidateNotNullOrEmpty()]
        public string[] Scope { get; set; } = new[] { ".default" };

        [Parameter(ParameterSetName = "NonInteractive")]
        [Parameter(ParameterSetName = "Interactive")]
        [Parameter(ParameterSetName = "ManagedIdentity")]
        [ValidateNotNullOrEmpty()]
        public string TenantId { get; set; }

        [Parameter(ParameterSetName = "NonInteractive")]
        [Parameter(ParameterSetName = "Interactive")]
        [Parameter(ParameterSetName = "ManagedIdentity")]
        [ValidateNotNullOrEmpty()]
        public string Claim { get; set; }

        [Parameter(ParameterSetName = "Interactive")]
        [Parameter(ParameterSetName = "ManagedIdentity")]
        public string ClientId { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Interactive")]
        public SwitchParameter Interactive { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "ManagedIdentity")]
        public SwitchParameter ManagedIdentity { get; set; }

        [Parameter(ParameterSetName = "NonInteractive")]
        [Parameter(ParameterSetName = "Interactive")]
        [Parameter(ParameterSetName = "ManagedIdentity")]
        public SwitchParameter Force { get; set; }

        private protected CancellationTokenSource cancellationTokenSource = new();

        // If user specifies Force, disregard earlier authentication
        protected override void BeginProcessing()
        {
            if (Force.IsPresent)
            {
                TokenManager.ClearCredential();
            }
        }

        // Cancel any operations if user presses CTRL + C
        protected override void StopProcessing() => cancellationTokenSource.Cancel();

        protected override void ProcessRecord()
        {
            WriteVerbose($"Getting token for {Resource} with scopes: {string.Join(", ", Scope)}.");

            if (ParameterSetName == "NonInteractive")
            {
                WriteVerbose(@"Looking for a token from the following sources:
Environment variables (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.environmentcredential)
Azure PowerShell (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.azurepowershellcredential)
Azure CLI (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.azureclicredential)
Visual Studio Code (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.visualstudiocodecredential)
Visual Studio (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.visualstudiocredential)
");
                WriteObject(TokenManager.GetTokenNonInteractive(Resource, Scope, Claim, TenantId, cancellationTokenSource.Token));
            }
            else if (Interactive.IsPresent)
            {
                WriteVerbose("Getting token interactively using the default browser (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.interactivebrowsercredential).");
                WriteObject(TokenManager.GetTokenInteractive(Resource, Scope, Claim, ClientId, TenantId, cancellationTokenSource.Token));
            }
            else if (ManagedIdentity.IsPresent)
            {
                WriteVerbose("Getting token as managed identity (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.managedidentitycredential).");
                WriteObject(TokenManager.GetTokenManagedIdentity(Resource, Scope, Claim, ClientId, TenantId, cancellationTokenSource.Token));
            }
            else
            {
                throw new ArgumentException("Invalid parameter combination!");
            }
        }
    }
}