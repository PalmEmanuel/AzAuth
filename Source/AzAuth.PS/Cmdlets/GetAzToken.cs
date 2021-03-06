using AzAuth.Core;
using System.Management.Automation;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace AzAuth.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "AzToken")]
    public class GetAzToken : PSCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "NonInteractive")]
        [Parameter(Mandatory = true, ParameterSetName = "Interactive")]
        [Parameter(Mandatory = true, ParameterSetName = "ManagedIdentity")]
        [ValidateNotNullOrEmpty()]
        [Alias("ResourceId", "ResourceUrl")]
        public string Resource { get; set; }

        [Parameter(ParameterSetName = "NonInteractive")]
        [Parameter(ParameterSetName = "Interactive")]
        [Parameter(ParameterSetName = "ManagedIdentity")]
        [ValidateNotNullOrEmpty()]
        public string[] Scopes { get; set; } = new[] { ".default" };

        [Parameter(ParameterSetName = "NonInteractive")]
        [Parameter(ParameterSetName = "Interactive")]
        [Parameter(ParameterSetName = "ManagedIdentity")]
        [ValidateNotNullOrEmpty()]
        public string TenantId { get; set; }

        [Parameter(ParameterSetName = "NonInteractive")]
        [Parameter(ParameterSetName = "Interactive")]
        [Parameter(ParameterSetName = "ManagedIdentity")]
        [ValidateNotNullOrEmpty()]
        public string Claims { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Interactive")]
        public SwitchParameter Interactive { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "ManagedIdentity")]
        public SwitchParameter ManagedIdentity { get; set; }

        protected override void ProcessRecord()
        {
            WriteVerbose($"Getting token for {Resource} with scopes: {string.Join(", ", Scopes)}.");

            if (ParameterSetName == "NonInteractive")
            {
                WriteVerbose(@"Looking for a token from the following sources:
Environment variables (https://docs.microsoft.com/en-us/dotnet/api/azure.identity.environmentcredential?view=azure-dotnet)
Shared token cache (https://docs.microsoft.com/en-us/dotnet/api/azure.identity.sharedtokencachecredential?view=azure-dotnet)
Azure PowerShell (https://docs.microsoft.com/en-us/dotnet/api/azure.identity.azurepowershellcredential?view=azure-dotnet)
Azure CLI (https://docs.microsoft.com/en-us/dotnet/api/azure.identity.azureclicredential?view=azure-dotnet)
Visual Studio Code (https://docs.microsoft.com/en-us/dotnet/api/azure.identity.visualstudiocodecredential?view=azure-dotnet)
Visual Studio (https://docs.microsoft.com/en-us/dotnet/api/azure.identity.visualstudiocredential?view=azure-dotnet)
");
                WriteObject(TokenManager.GetToken(Resource, Scopes, Claims, TenantId));
            }
            else if (Interactive.IsPresent)
            {
                WriteVerbose("Getting token interactively using the default browser (https://docs.microsoft.com/en-us/dotnet/api/azure.identity.interactivebrowsercredential?view=azure-dotnet).");
                WriteObject(TokenManager.GetTokenInteractive(Resource, Scopes, Claims, TenantId));
            }
            else if (ManagedIdentity.IsPresent)
            {
                WriteVerbose("Getting token as managed identity (https://docs.microsoft.com/en-us/dotnet/api/azure.identity.managedidentitycredential?view=azure-dotnet).");
                WriteObject(TokenManager.GetTokenManagedIdentity(Resource, Scopes, Claims, TenantId));
            }
            else
            {
                throw new ArgumentException("Invalid parameter combination!");
            }
        }
    }
}