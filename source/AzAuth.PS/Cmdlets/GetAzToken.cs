using System.Collections.Concurrent;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace PipeHow.AzAuth;

[Cmdlet(VerbsCommon.Get, "AzToken", DefaultParameterSetName = "NonInteractive")]
public class GetAzToken : PSLoggerCmdletBase
{
    [Parameter(ParameterSetName = "NonInteractive", Position = 0)]
    [Parameter(ParameterSetName = "Cache", Position = 0)]
    [Parameter(ParameterSetName = "Interactive", Position = 0)]
    [Parameter(ParameterSetName = "Broker", Position = 0)]
    [Parameter(ParameterSetName = "DeviceCode", Position = 0)]
    [Parameter(ParameterSetName = "ManagedIdentity", Position = 0)]
    [Parameter(ParameterSetName = "WorkloadIdentity", Position = 0)]
    [Parameter(ParameterSetName = "ClientSecret", Position = 0)]
    [Parameter(ParameterSetName = "ClientCertificate", Position = 0)]
    [Parameter(ParameterSetName = "ClientCertificatePath", Position = 0)]
    [ValidateNotNullOrEmpty]
    [Alias("ResourceId", "ResourceUrl")]
    public string Resource { get; set; } = "https://graph.microsoft.com";

    [Parameter(ParameterSetName = "NonInteractive", Position = 1)]
    [Parameter(ParameterSetName = "Cache", Position = 1)]
    [Parameter(ParameterSetName = "Interactive", Position = 1)]
    [Parameter(ParameterSetName = "Broker", Position = 1)]
    [Parameter(ParameterSetName = "DeviceCode", Position = 1)]
    [Parameter(ParameterSetName = "ManagedIdentity", Position = 1)]
    [Parameter(ParameterSetName = "WorkloadIdentity", Position = 1)]
    [Parameter(ParameterSetName = "ClientSecret", Position = 1)]
    [Parameter(ParameterSetName = "ClientCertificate", Position = 1)]
    [Parameter(ParameterSetName = "ClientCertificatePath", Position = 1)]
    [ValidateNotNullOrEmpty]
    public string[] Scope { get; set; } = new[] { ".default" };

    [Parameter(ParameterSetName = "NonInteractive")]
    [Parameter(ParameterSetName = "Cache")]
    [Parameter(ParameterSetName = "Interactive")]
    [Parameter(ParameterSetName = "Broker")]
    [Parameter(ParameterSetName = "DeviceCode")]
    [Parameter(ParameterSetName = "ManagedIdentity")]
    [Parameter(ParameterSetName = "WorkloadIdentity", Mandatory = true)]
    [Parameter(ParameterSetName = "ClientSecret", Mandatory = true)]
    [Parameter(ParameterSetName = "ClientCertificate", Mandatory = true)]
    [Parameter(ParameterSetName = "ClientCertificatePath", Mandatory = true)]
    [ValidateNotNullOrEmpty]
    [Alias("TenantId")]
    public string Tenant { get; set; }

    [Parameter(ParameterSetName = "NonInteractive")]
    [Parameter(ParameterSetName = "Cache")]
    [Parameter(ParameterSetName = "Interactive")]
    [Parameter(ParameterSetName = "Broker")]
    [Parameter(ParameterSetName = "DeviceCode")]
    [Parameter(ParameterSetName = "ManagedIdentity")]
    [Parameter(ParameterSetName = "WorkloadIdentity")]
    [Parameter(ParameterSetName = "ClientSecret")]
    [Parameter(ParameterSetName = "ClientCertificate")]
    [Parameter(ParameterSetName = "ClientCertificatePath")]
    [ValidateNotNullOrEmpty]
    public string Claim { get; set; }

    [Parameter(ParameterSetName = "NonInteractive")]
    [Parameter(ParameterSetName = "Interactive")]
    [Parameter(ParameterSetName = "Broker")]
    [Parameter(ParameterSetName = "DeviceCode")]
    [Parameter(ParameterSetName = "ManagedIdentity")]
    [Parameter(ParameterSetName = "Cache")]
    [Parameter(ParameterSetName = "WorkloadIdentity", Mandatory = true)]
    [Parameter(ParameterSetName = "ClientSecret", Mandatory = true)]
    [Parameter(ParameterSetName = "ClientCertificate", Mandatory = true)]
    [Parameter(ParameterSetName = "ClientCertificatePath", Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public string ClientId { get; set; }

    [Parameter(ParameterSetName = "Interactive")]
    [Parameter(ParameterSetName = "DeviceCode")]
    [Parameter(ParameterSetName = "Cache", Mandatory = true)]
    [ValidateNotNullOrEmpty]
    [ArgumentCompleter(typeof(ExistingCaches))]
    public string TokenCache { get; set; }

    [Parameter(ParameterSetName = "Interactive")]
    [Parameter(ParameterSetName = "DeviceCode")]
    [Parameter(ParameterSetName = "Cache")]
    public SwitchParameter UseUnprotectedTokenCache { get; set; }

    [Parameter(ParameterSetName = "Cache", Mandatory = true)]
    [ValidateNotNullOrEmpty]
    [ArgumentCompleter(typeof(ExistingAccounts))]
    public string Username { get; set; }

    [Parameter(ParameterSetName = "NonInteractive")]
    [Parameter(ParameterSetName = "Interactive")]
    [Parameter(ParameterSetName = "ManagedIdentity")]
    [Parameter(ParameterSetName = "DeviceCode")]
    [ValidateRange(1, int.MaxValue)]
    public int TimeoutSeconds { get; set; } = 120;

    [Parameter(ParameterSetName = "NonInteractive")]
    [ValidateSet("ManagedIdentity", "Environment", "AzurePowerShell", "AzureCLI", "VisualStudio", "SharedTokenCache")]
    [ValidateNotNullOrEmpty()]
    // TODO: Change back ManagedIdentity to first position in the chain once issue #112 is solved, likely in Azure.Identity 1.14.2 or later
    public string[] CredentialPrecedence { get; set; } = ["Environment", "AzurePowerShell", "AzureCLI", "VisualStudio", "SharedTokenCache", "ManagedIdentity"];

    [Parameter(ParameterSetName = "Interactive", Mandatory = true)]
    public SwitchParameter Interactive { get; set; }

    [Parameter(ParameterSetName = "Broker", Mandatory = true)]
    public SwitchParameter Broker { get; set; }

    [Parameter(ParameterSetName = "DeviceCode", Mandatory = true)]
    public SwitchParameter DeviceCode { get; set; }

    [Parameter(ParameterSetName = "ManagedIdentity", Mandatory = true)]
    public SwitchParameter ManagedIdentity { get; set; }

    [Parameter(ParameterSetName = "WorkloadIdentity", Mandatory = true)]
    public SwitchParameter WorkloadIdentity { get; set; }

    [Parameter(ParameterSetName = "WorkloadIdentity", Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public string ExternalToken { get; set; }

    [Parameter(ParameterSetName = "ClientSecret", Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public string ClientSecret { get; set; }

    [Parameter(ParameterSetName = "ClientCertificate", Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public X509Certificate2 ClientCertificate { get; set; }

    [Parameter(ParameterSetName = "ClientCertificatePath", Mandatory = true)]
    [ValidateNotNullOrEmpty]
    [ValidateCertificatePath]
    public string ClientCertificatePath { get; set; }

    [Parameter(ParameterSetName = "NonInteractive")]
    [Parameter(ParameterSetName = "Interactive")]
    [Parameter(ParameterSetName = "DeviceCode")]
    [Parameter(ParameterSetName = "ManagedIdentity")]
    [Parameter(ParameterSetName = "WorkloadIdentity")]
    [Parameter(ParameterSetName = "ClientSecret")]
    [Parameter(ParameterSetName = "ClientCertificate")]
    [Parameter(ParameterSetName = "ClientCertificatePath")]
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
        string cacheRootDir = CacheManager.GetCacheRootDirectory();
        if (TokenCache == "msal.cache")
        {
            WriteWarning("The name 'msal.cache' is the default cache name in MSAL, changing or clearing this cache might break other tools using it!");
        }

        WriteVerbose($"Getting token for {Resource} with scopes: {string.Join(", ", Scope)}.");

        if (UseUnprotectedTokenCache.IsPresent)
        {
            if (!(Interactive.IsPresent || DeviceCode.IsPresent) || !MyInvocation.BoundParameters.ContainsKey("TokenCache"))
            {
                throw new ArgumentException("The -UseUnprotectedTokenCache switch can only be used with -TokenCache in combination with -Interactive or -DeviceCode authentication methods!");
            }

            WriteWarning("Unprotected token caches store tokens as plain text on the file system! Only use this in secure environments at your own risk!");
        }

        if (ParameterSetName == "NonInteractive")
        {
            if (MyInvocation.BoundParameters.ContainsKey("ClientId"))
            {
                if (TokenManager.HasClientId())
                {
                    WriteWarning(@"The ClientId is saved in the session from the previous interactive authentication. If you wish to use the same authentication, omit the ClientId parameter and any parameters indicating interactive authentication. For example:
'Get-AzToken -Interactive -ClientId $ClientId -Resource $Resource -Scope $Scope'

should be

'Get-AzToken -Resource $Resource -Scope $Scope'");
                }
                throw new ArgumentException("The ClientId parameter is not supported for this parameter combination.");
            }

            // If user didn't specify a timeout, set default for managed identity
            int managedIdentityTimeoutSeconds = 1;
            int? noninteractiveTimeoutSeconds = null;
            if (MyInvocation.BoundParameters.ContainsKey("TimeoutSeconds"))
            {
                managedIdentityTimeoutSeconds = TimeoutSeconds;
                noninteractiveTimeoutSeconds = TimeoutSeconds;
            }

            CredentialPrecedence = CredentialPrecedence.Distinct().ToArray();

            WriteVerbose(@$"Looking for a token from the following sources:
{string.Join(Environment.NewLine, CredentialPrecedence.Select(cred => $"{cred} ({TokenManager.GetCredentialDocumentationUrl(cred)})"))}");
            WriteObject(TokenManager.GetTokenNonInteractive(Resource, Scope, Claim, Tenant, CredentialPrecedence, noninteractiveTimeoutSeconds, managedIdentityTimeoutSeconds, stopProcessing.Token));
        }
        else if (ParameterSetName == "Cache")
        {
            WriteVerbose($"Getting token from token cache named \"{TokenCache}\".");
            WriteObject(TokenManager.GetTokenFromCache(Resource, Scope, Claim, ClientId, Tenant, TokenCache!, cacheRootDir, Username, UseUnprotectedTokenCache.IsPresent, stopProcessing.Token));
        }
        else if (Interactive.IsPresent)
        {
            WriteVerbose("Getting token interactively using the default browser.");
            WriteObject(TokenManager.GetTokenInteractive(Resource, Scope, Claim, ClientId, Tenant, TokenCache, cacheRootDir, TimeoutSeconds, UseUnprotectedTokenCache.IsPresent, stopProcessing.Token));
        }
        else if (Broker.IsPresent)
        {
            WriteVerbose("Getting token interactively using the WAM broker.");
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("The WAM broker authentication is only supported on Windows.");
            }
            WriteObject(TokenManager.GetTokenInteractiveBroker(Resource, Scope, Claim, ClientId, Tenant, TimeoutSeconds, stopProcessing.Token));
        }
        else if (DeviceCode.IsPresent)
        {
            WriteVerbose("Getting token using the device code flow.");

            // Set up a BlockingCollection to use for logging device code message
            BlockingCollection<string> loggingQueue = new();
            // Start device code flow and save task
            var tokenTask = joinableTaskFactory.RunAsync(() => TokenManager.GetTokenDeviceCodeAsync(Resource, Scope, Claim, ClientId, Tenant, TokenCache, cacheRootDir, TimeoutSeconds, UseUnprotectedTokenCache.IsPresent, loggingQueue, stopProcessing.Token));

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
            // If user didn't specify a timeout, default to 1 second for managed identity
            if (!MyInvocation.BoundParameters.ContainsKey("TimeoutSeconds"))
            {
                TimeoutSeconds = 1;
            }
            WriteVerbose("Getting token using a managed identity (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.managedidentitycredential).");
            WriteObject(TokenManager.GetTokenManagedIdentity(Resource, Scope, Claim, ClientId, Tenant, TimeoutSeconds, stopProcessing.Token));
        }
        else if (WorkloadIdentity.IsPresent)
        {
            WriteVerbose($"Getting token using workload identity federation (using client assertion) for client \"{ClientId}\" (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.clientassertioncredential).");
            WriteObject(TokenManager.GetTokenWorkloadIdentity(Resource, Scope, Claim, ClientId, Tenant, ExternalToken, stopProcessing.Token));
        }
        else if (ParameterSetName == "ClientSecret")
        {
            WriteVerbose($"Getting token using client secret for client \"{ClientId}\" (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.clientsecretcredential).");
            WriteObject(TokenManager.GetTokenClientSecret(Resource, Scope, Claim, ClientId, Tenant, ClientSecret, stopProcessing.Token));
        }
        else if (ParameterSetName == "ClientCertificate")
        {
            WriteVerbose($"Getting token using client certificate for client \"{ClientId}\" (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.clientcertificatecredential).");
            WriteObject(TokenManager.GetTokenClientCertificate(Resource, Scope, Claim, ClientId, Tenant, ClientCertificate, stopProcessing.Token));
        }
        else if (ParameterSetName == "ClientCertificatePath")
        {
            WriteVerbose($"Getting token using client certificate for client \"{ClientId}\" (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.clientcertificatecredential).");
            WriteObject(TokenManager.GetTokenClientCertificate(Resource, Scope, Claim, ClientId, Tenant, ClientCertificatePath, stopProcessing.Token));
        }
        else
        {
            throw new ArgumentException("Invalid parameter combination!");
        }
    }
}