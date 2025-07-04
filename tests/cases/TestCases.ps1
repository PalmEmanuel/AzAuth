# Azure PowerShell Client ID
$AzurePowerShellClientId = '1950a258-227b-4e31-a9cf-717495945fc2'

# Define authentication flows
$AuthFlows = @(
    @{
        Name = 'Interactive'
        Parameters = @{ Interactive = $true }
        RequiresUserInteraction = $true
        Tag = 'Interactive'
    },
    @{
        Name = 'DeviceCode'
        Parameters = @{ DeviceCode = $true }
        RequiresUserInteraction = $true
        Tag = 'Interactive'
    },
    @{
        Name = 'ClientSecret'
        Parameters = @{ 
            ClientId = $env:AZURE_CLIENT_ID
            ClientSecret = $env:AZURE_CLIENT_SECRET
            TenantId = $env:AZURE_TENANT_ID
        }
        RequiresUserInteraction = $false
        Tag = 'Integration'
        RequiresSecrets = $true
    },
    @{
        Name = 'ClientCertificate'
        Parameters = @{
            ClientId = $env:AZURE_CLIENT_ID
            CertificatePath = $env:TEST_CERTIFICATE_PATH
            TenantId = $env:AZURE_TENANT_ID
        }
        RequiresUserInteraction = $false
        Tag = 'Integration'
        RequiresSecrets = $true
    },
    @{
        Name = 'ManagedIdentity'
        Parameters = @{ ManagedIdentity = $true }
        RequiresUserInteraction = $false
        Tag = 'Integration'
        RequiresAzureEnvironment = $true
    }
)

# Define test resources with different scenarios
$TestResources = @(
    @{
        Name = 'GraphDefault'
        Resource = 'https://graph.microsoft.com'
        Scopes = @('.default')
        Description = 'Microsoft Graph API'
        ExpectedAudience = 'https://graph.microsoft.com'
    },
    @{
        Name = 'AzureResourceManager'
        Resource = 'https://management.azure.com'
        Scopes = @('.default')
        Description = 'Azure Resource Manager'
        ExpectedAudience = 'https://management.azure.com'
    },
    @{
        Name = 'KeyVault'
        Resource = 'https://vault.azure.net'
        Scopes = @('.default')
        Description = 'Azure Key Vault'
        ExpectedAudience = 'https://vault.azure.net'
    },
    @{
        Name = 'Storage'
        Resource = 'https://storage.azure.com'
        Scopes = @('.default')
        Description = 'Azure Storage'
        ExpectedAudience = 'https://storage.azure.com'
    },
    @{
        Name = 'GraphSpecificScopes'
        Resource = 'https://graph.microsoft.com'
        Scopes = @('User.Read', 'Mail.Read')
        Description = 'Microsoft Graph with specific scopes'
        ExpectedAudience = 'https://graph.microsoft.com'
    }
)

# Define tenant scenarios
$TenantScenarios = @(
    @{
        Name = 'CommonTenant'
        TenantId = 'common'
        Description = 'Use default tenant'
    },
    @{
        Name = 'SpecificTenant'
        TenantId = $env:AZURE_TENANT_ID
        Description = 'Use specific tenant ID'
    }
)

# Generate test matrix combinations
function Get-TestMatrix {
    param(
        [string[]]$IncludeFlows = @('Interactive', 'DeviceCode', 'ClientSecret', 'Certificate', 'ManagedIdentity'),
        [string[]]$IncludeResources = @('MicrosoftGraph', 'AzureResourceManager'),
        [string[]]$IncludeTenants = @('DefaultTenant'),
        [string[]]$Tags = @()
    )
    
    $TestCases = @()
    
    foreach ($flow in $AuthFlows | Where-Object { $_.Name -in $IncludeFlows }) {
        foreach ($resource in $TestResources | Where-Object { $_.Name -in $IncludeResources }) {
            foreach ($tenant in $TenantScenarios | Where-Object { $_.Name -in $IncludeTenants }) {
                
                # Skip combinations that don't make sense
                if ($flow.RequiresSecrets -and -not $env:AZURE_CLIENT_ID) {
                    continue
                }
                
                if ($tenant.RequiresSecrets -and -not $env:AZURE_TENANT_ID) {
                    continue
                }
                
                if ($Tags.Count -gt 0 -and $flow.Tag -notin $Tags) {
                    continue
                }
                
                $testCase = @{
                    TestName = "$($flow.Name)_$($resource.Name)_$($tenant.Name)"
                    FlowName = $flow.Name
                    ResourceName = $resource.Name
                    TenantName = $tenant.Name
                    
                    # Auth parameters
                    AuthParameters = $flow.Parameters.Clone()
                    
                    # Resource parameters
                    Resource = $resource.Resource
                    Scopes = $resource.Scopes
                    ExpectedAudience = $resource.ExpectedAudience
                    
                    # Tenant parameters
                    TenantId = $tenant.TenantId
                    
                    # Test metadata
                    RequiresUserInteraction = $flow.RequiresUserInteraction
                    RequiresSecrets = $flow.RequiresSecrets -or $tenant.RequiresSecrets
                    RequiresAzureEnvironment = $flow.RequiresAzureEnvironment
                    Tag = $flow.Tag
                    
                    # Description for test output
                    Description = "Authenticate using $($flow.Name) for $($resource.Description) in $($tenant.Description)"
                }
                
                # Add tenant ID to auth parameters if specified
                if ($tenant.TenantId) {
                    $testCase.AuthParameters.TenantId = $tenant.TenantId
                }
                
                $TestCases += $testCase
            }
        }
    }
    
    return $TestCases
}