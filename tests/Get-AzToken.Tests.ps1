BeforeDiscovery {    
    $ParameterTestCases += @(
        @{
            Name          = 'Resource'
            Type          = 'string'
            ParameterSets = @(
                @{ Name = 'NonInteractive'; Mandatory = $false }
                @{ Name = 'Cache'; Mandatory = $false }
                @{ Name = 'Interactive'; Mandatory = $false }
                @{ Name = 'Broker'; Mandatory = $false }
                @{ Name = 'DeviceCode'; Mandatory = $false }
                @{ Name = 'ManagedIdentity'; Mandatory = $false }
                @{ Name = 'WorkloadIdentity'; Mandatory = $false }
                @{ Name = 'ClientSecret'; Mandatory = $false }
                @{ Name = 'ClientCertificate'; Mandatory = $false }
                @{ Name = 'ClientCertificatePath'; Mandatory = $false }
            )
        }
        @{
            Name          = 'Scope'
            Type          = 'string[]'
            ParameterSets = @(
                @{ Name = 'NonInteractive'; Mandatory = $false }
                @{ Name = 'Cache'; Mandatory = $false }
                @{ Name = 'Interactive'; Mandatory = $false }
                @{ Name = 'Broker'; Mandatory = $false }
                @{ Name = 'DeviceCode'; Mandatory = $false }
                @{ Name = 'ManagedIdentity'; Mandatory = $false }
                @{ Name = 'WorkloadIdentity'; Mandatory = $false }
                @{ Name = 'ClientSecret'; Mandatory = $false }
                @{ Name = 'ClientCertificate'; Mandatory = $false }
                @{ Name = 'ClientCertificatePath'; Mandatory = $false }
            )
        }
        @{
            Name          = 'TenantId'
            Type          = 'string'
            ParameterSets = @(
                @{ Name = 'NonInteractive'; Mandatory = $false }
                @{ Name = 'Cache'; Mandatory = $false }
                @{ Name = 'Interactive'; Mandatory = $false }
                @{ Name = 'Broker'; Mandatory = $false }
                @{ Name = 'DeviceCode'; Mandatory = $false }
                @{ Name = 'ManagedIdentity'; Mandatory = $false }
                @{ Name = 'WorkloadIdentity'; Mandatory = $true }
                @{ Name = 'ClientSecret'; Mandatory = $true }
                @{ Name = 'ClientCertificate'; Mandatory = $true }
                @{ Name = 'ClientCertificatePath'; Mandatory = $true }
            )
        }
        @{
            Name          = 'Claim'
            Type          = 'string'
            ParameterSets = @(
                @{ Name = 'NonInteractive'; Mandatory = $false }
                @{ Name = 'Cache'; Mandatory = $false }
                @{ Name = 'Interactive'; Mandatory = $false }
                @{ Name = 'Broker'; Mandatory = $false }
                @{ Name = 'DeviceCode'; Mandatory = $false }
                @{ Name = 'ManagedIdentity'; Mandatory = $false }
                @{ Name = 'WorkloadIdentity'; Mandatory = $false }
                @{ Name = 'ClientSecret'; Mandatory = $false }
                @{ Name = 'ClientCertificate'; Mandatory = $false }
                @{ Name = 'ClientCertificatePath'; Mandatory = $false }
            )
        }
        @{
            Name          = 'ClientId'
            Type          = 'string'
            ParameterSets = @(
                @{ Name = 'NonInteractive'; Mandatory = $false }
                @{ Name = 'Interactive'; Mandatory = $false }
                @{ Name = 'Broker'; Mandatory = $false }
                @{ Name = 'DeviceCode'; Mandatory = $false }
                @{ Name = 'ManagedIdentity'; Mandatory = $false }
                @{ Name = 'Cache'; Mandatory = $false }
                @{ Name = 'WorkloadIdentity'; Mandatory = $true }
                @{ Name = 'ClientSecret'; Mandatory = $true }
                @{ Name = 'ClientCertificate'; Mandatory = $true }
                @{ Name = 'ClientCertificatePath'; Mandatory = $true }
            )
        }
        @{
            Name          = 'TokenCache'
            Type          = 'string'
            ParameterSets = @(
                @{ Name = 'Interactive'; Mandatory = $false }
                @{ Name = 'DeviceCode'; Mandatory = $false }
                @{ Name = 'Cache'; Mandatory = $true }
            )
        }
        @{
            Name          = 'Username'
            Type          = 'string'
            ParameterSets = @(
                @{ Name = 'Cache'; Mandatory = $true }
            )
        }
        @{
            Name          = 'Broker'
            Type          = 'System.Management.Automation.SwitchParameter'
            ParameterSets = @(
                @{ Name = 'Broker'; Mandatory = $true }
            )
        }
        @{
            Name          = 'TimeoutSeconds'
            Type          = 'int'
            ParameterSets = @(
                @{ Name = 'NonInteractive'; Mandatory = $false }
                @{ Name = 'Interactive'; Mandatory = $false }
                @{ Name = 'ManagedIdentity'; Mandatory = $false }
                @{ Name = 'DeviceCode'; Mandatory = $false }
            )
        }
        @{
            Name          = 'Interactive'
            Type          = 'System.Management.Automation.SwitchParameter'
            ParameterSets = @(
                @{ Name = 'Interactive'; Mandatory = $true }
            )
        }
        @{
            Name          = 'DeviceCode'
            Type          = 'System.Management.Automation.SwitchParameter'
            ParameterSets = @(
                @{ Name = 'DeviceCode'; Mandatory = $true }
            )
        }
        @{
            Name          = 'ManagedIdentity'
            Type          = 'System.Management.Automation.SwitchParameter'
            ParameterSets = @(
                @{ Name = 'ManagedIdentity'; Mandatory = $true }
            )
        }
        @{
            Name          = 'WorkloadIdentity'
            Type          = 'System.Management.Automation.SwitchParameter'
            ParameterSets = @(
                @{ Name = 'WorkloadIdentity'; Mandatory = $true }
            )
        }
        @{
            Name          = 'ExternalToken'
            Type          = 'string'
            ParameterSets = @(
                @{ Name = 'WorkloadIdentity'; Mandatory = $true }
            )
        }
        @{
            Name          = 'ClientSecret'
            Type          = 'string'
            ParameterSets = @(
                @{ Name = 'ClientSecret'; Mandatory = $true }
            )
        }
        @{
            Name          = 'ClientCertificate'
            Type          = 'System.Security.Cryptography.X509Certificates.X509Certificate2'
            ParameterSets = @(
                @{ Name = 'ClientCertificate'; Mandatory = $true }
            )
        }
        @{
            Name          = 'ClientCertificatePath'
            Type          = 'string'
            ParameterSets = @(
                @{ Name = 'ClientCertificatePath'; Mandatory = $true }
            )
        }
        @{
            Name          = 'Force'
            Type          = 'System.Management.Automation.SwitchParameter'
            ParameterSets = @(
                @{ Name = 'NonInteractive'; Mandatory = $false }
                @{ Name = 'Interactive'; Mandatory = $false }
                @{ Name = 'DeviceCode'; Mandatory = $false }
                @{ Name = 'ManagedIdentity'; Mandatory = $false }
                @{ Name = 'WorkloadIdentity'; Mandatory = $false }
                @{ Name = 'ClientSecret'; Mandatory = $false }
                @{ Name = 'ClientCertificate'; Mandatory = $false }
                @{ Name = 'ClientCertificatePath'; Mandatory = $false }
            )
        }
    )
}

Describe 'Get-AzToken' {
    BeforeAll {        
        # Get command from current test file name
        $Command = Get-Command ((Split-Path $PSCommandPath -Leaf) -replace '.Tests.ps1')
    }

    Context 'parameters' {
        It 'only has expected parameters' -TestCases @{ Parameters = $ParameterTestCases.Name } {
            $Command.Parameters.GetEnumerator() | Where-Object {
                $_.Key -notin [System.Management.Automation.Cmdlet]::CommonParameters -and
                $_.Key -notin $Parameters
            } | Should -BeNullOrEmpty
        }

        It 'has parameter <Name> of type <Type>' -TestCases $ParameterTestCases {
            $Command | Should -HaveParameter $Name -Type $Type
        }

        It 'has correct parameter sets for parameter <Name>' -TestCases $ParameterTestCases {
            $Parameter = $Command.Parameters[$Name]
            $Parameter.ParameterSets.Keys | Should -BeExactly $ParameterSets.Name
        }

        foreach ($ParameterTestCase in $ParameterTestCases) {
            foreach ($ParameterSet in $ParameterTestCase.ParameterSets) {
                It 'has parameter <ParameterName> set to mandatory <Mandatory> for parameter set <Name>' -TestCases @{
                    ParameterName = $ParameterTestCase['Name']
                    Name          = $ParameterSet['Name']
                    Mandatory     = $ParameterSet['Mandatory']
                } {
                    $Parameter = $Command.Parameters[$ParameterName]
                    $Parameter.ParameterSets[$Name].IsMandatory | Should -Be $Mandatory
                }
            }
        }
    }
}