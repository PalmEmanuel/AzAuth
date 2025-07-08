# Azure PowerShell Client ID
$AzurePowerShellClientId = '1950a258-227b-4e31-a9cf-717495945fc2'

# Define common parameters for all test cases
$CommonSplat = @{
    Tenant   = 'common'
    ClientId = $AzurePowerShellClientId
}

# Define specific parameters for each authentication flow scenario
# Make sure they're ordered to ensure consistent test execution
$InteractiveAuthFlows = [ordered]@{
    'Interactive'            = @{
        Interactive   = $true
        Resource      = 'https://graph.microsoft.com'
        Scope         = @('.default')
        ExpectedScope = 'openid'
    }
    'Interactive with Cache' = @{
        Interactive   = $true
        TokenCache    = 'IntegrationInteractiveTestCache'
        Resource      = 'https://management.azure.com'
        Scope         = @('.default')
        ExpectedScope = 'https://management.azure.com/user_impersonation'
    }
    'Cache from Interactive' = @{
        TokenCache    = 'IntegrationInteractiveTestCache'
        Username      = '' # Will be set by the test
        Resource      = 'cfa8b339-82a2-471a-a3c9-0fc0be7a4093' # Azure Key Vault
        Scope         = @('.default')
        ExpectedScope = 'cfa8b339-82a2-471a-a3c9-0fc0be7a4093/user_impersonation'
    }
    # 'Device Code'            = @{
    #     DeviceCode = $true
    # }
    'Device Code with Cache' = @{
        DeviceCode   = $true
        TokenCache    = 'IntegrationDeviceCodeTestCache'
        Resource      = 'https://storage.azure.com'
        Scope         = @('.default')
        ExpectedScope = 'https://storage.azure.com/user_impersonation'
    }
    'Cache from Device Code' = @{
        TokenCache    = 'IntegrationDeviceCodeTestCache'
        Username      = '' # Will be set by the test
        Resource      = 'https://database.windows.net' # Azure SQL Database
        Scope         = @('.default')
        ExpectedScope = 'https://database.windows.net/.default'
    }
}

# Generate test splatting hashtable based on test cases
function Get-TestSplat {
    param(
        [Parameter(Mandatory)]
        [ValidateSet(
            'Interactive',
            'Interactive with Cache',
            'Cache from Interactive',
            'Device Code',
            'Device Code with Cache',
            'Cache from Device Code'
        )]
        [string]$Flow
    )
    
    $TestSplat = $CommonSplat.Clone()
    # Transfer all parameters from the testcase to the splat
    foreach ($AuthFlowKey in $InteractiveAuthFlows[$Flow].Keys) {
        $TestSplat[$AuthFlowKey] = $InteractiveAuthFlows[$Flow][$AuthFlowKey]
    }
    
    return $TestSplat.Clone()
}

function Get-TestCaseByType {
    param(
        [Parameter(Mandatory)]
        [ValidateSet('InteractiveTests', 'NonInteractiveTests', 'CITests')]
        [string[]]$CaseType
    )

    $AuthFlows = [ordered]@{}

    if ('InteractiveTests' -in $CaseType) {
        # Add interactive test cases
        $AuthFlows += $InteractiveAuthFlows
    }
    if ('NonInteractiveTests' -in $CaseType) {
        # Add non-interactive test cases
        $AuthFlows += @{
        }
    }
    if ('CITests' -in $CaseType) {
        # Add CI tests (default)
        $AuthFlows += @{
        }
    }

    foreach ($Flow in $AuthFlows.Keys) {
        # Get test splat for the current flow
        $TestSplat = Get-TestSplat -Flow $Flow
        # Remove the expected scope from the splat to avoid passing it as a parameter
        $ExpectedScope = $TestSplat['ExpectedScope']
        $TestSplat.Remove('ExpectedScope')  # Remove ExpectedScope from splat
        @(
            @{
                Splat         = $TestSplat
                ExpectedScope = $ExpectedScope
                Flow          = $Flow
                Resource      = $TestSplat['Resource']
            }
        )
    }
}