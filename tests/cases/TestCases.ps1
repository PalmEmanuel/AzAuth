function New-ClientCertificateFromPem {
    param(
        [Parameter(Mandatory)]
        [string]$Certificate,

        [Parameter(Mandatory)]
        [string]$PrivateKey
    )

    try {
        [System.Security.Cryptography.X509Certificates.X509Certificate2]::CreateFromPem(
            $Certificate,
            $PrivateKey
        )
    }
    catch {
        throw "Failed to create certificate object, did you set the environment variables correctly? Error: $_"
    }
}

function Get-TestCaseByType {
    param(
        [Parameter(Mandatory)]
        [ValidateSet('InteractiveTests', 'NonInteractiveTests', 'CITests')]
        [string[]]$CaseType,

        [Parameter()]
        [string]$InteractiveClientId = '1950a258-227b-4e31-a9cf-717495945fc2' # Azure PowerShell Client ID
    )

    $TestCases = [ordered]@{}

    if ('InteractiveTests' -in $CaseType) {
        # Add interactive test cases
        $TestCases += [ordered]@{
            'Interactive'            = @{
                Tenant        = 'common'
                ClientId      = $InteractiveClientId
                Interactive   = $true
                Resource      = 'https://graph.microsoft.com'
                Scope         = @('.default')
                ExpectedScope = 'openid'
            }
            'Interactive with Cache' = @{
                Tenant        = 'common'
                ClientId      = $InteractiveClientId
                Interactive   = $true
                TokenCache    = 'IntegrationInteractiveTestCache'
                Resource      = 'https://management.azure.com'
                Scope         = @('.default')
                ExpectedScope = 'https://management.azure.com/user_impersonation'
            }
            'Cache from Interactive' = @{
                Tenant        = 'common'
                ClientId      = $InteractiveClientId
                TokenCache    = 'IntegrationInteractiveTestCache'
                Username      = '' # Will be set by the test
                Resource      = 'cfa8b339-82a2-471a-a3c9-0fc0be7a4093' # Azure Key Vault
                Scope         = @('.default')
                ExpectedScope = 'cfa8b339-82a2-471a-a3c9-0fc0be7a4093/user_impersonation'
            }
            'Device Code with Cache' = @{
                Tenant        = 'common'
                ClientId      = $InteractiveClientId
                DeviceCode    = $true
                TokenCache    = 'IntegrationDeviceCodeTestCache'
                Resource      = 'https://storage.azure.com'
                Scope         = @('.default')
                ExpectedScope = 'https://storage.azure.com/user_impersonation'
            }
            'Cache from Device Code' = @{
                Tenant        = 'common'
                ClientId      = $InteractiveClientId
                TokenCache    = 'IntegrationDeviceCodeTestCache'
                Username      = '' # Will be set by the test
                Resource      = 'https://database.windows.net' # Azure SQL Database
                Scope         = @('.default')
                ExpectedScope = 'https://database.windows.net/.default'
            }
        }
    }
    # if ('CITests' -in $CaseType) {
    #     $TestCases += @{
    #     }
    # }
    if ('NonInteractiveTests' -in $CaseType) {
        # Create or resolve the PEM file path
        $PemPath = try {
            (New-Item -Name 'azauth.pem' -Value @"
$env:CLIENT_CERTIFICATE
$env:CLIENT_CERTIFICATE_PRIVATE_KEY
"@).FullName
        }
        catch {
            (Resolve-Path 'azauth.pem').Path
        }

        $TestCases += [ordered]@{
            'Client Secret'      = @{
                Tenant        = $env:TENANT_ID
                ClientId      = $env:CLIENT_ID
                ClientSecret  = $env:CLIENT_SECRET
                Resource      = 'https://management.azure.com'
                Scope         = @('.default')
                ExpectedScope = 'https://management.azure.com/.default'
            }
            'Certificate Object' = @{
                Tenant            = $env:TENANT_ID
                ClientId          = $env:CLIENT_ID
                ClientCertificate = New-ClientCertificateFromPem -Certificate $env:CLIENT_CERTIFICATE -PrivateKey $env:CLIENT_CERTIFICATE_PRIVATE_KEY
                Resource          = 'https://graph.microsoft.com'
                Scope             = @('.default')
                ExpectedScope     = 'https://graph.microsoft.com/.default'
            }
            'Certificate Path'   = @{
                Tenant                = $env:TENANT_ID
                ClientId              = $env:CLIENT_ID
                ClientCertificatePath = $PemPath
                Resource              = 'https://storage.azure.com'
                Scope                 = @('.default')
                ExpectedScope         = 'https://storage.azure.com/.default'
            }
        }
    }

    foreach ($Case in $TestCases.Keys) {
        # Get test splat for the current flow
        $TestSplat = $TestCases[$Case]
        # Remove the expected scope from the splat to avoid passing it as a parameter
        $ExpectedScope = $TestSplat['ExpectedScope']
        $TestSplat.Remove('ExpectedScope')  # Remove ExpectedScope from splat

        # Output the test case
        @{
            Splat         = $TestSplat
            ExpectedScope = $ExpectedScope
            Flow          = $Case
            Resource      = $TestSplat['Resource']
        }
    }
}