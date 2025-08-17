Describe 'Get-AzToken non-interactive integration tests' -Tag 'NonInteractive', 'Integration' {
    BeforeDiscovery {
        . "$BuildRoot\tests\cases\TestCases.ps1"
        $NonInteractiveTestCases = Get-TestCaseByType -CaseType 'NonInteractiveTests'
    }

    BeforeAll {
        function Get-UnsetRequiredEnvironmentVariable {
            param(
                [Parameter(Mandatory, Position = 0)]
                [string[]]$VariableName
            )

            $UnsetVars = foreach ($Var in $VariableName) {
                if (Test-Path "env:/$Var") { continue }
                
                Write-Output $Var
            }

            return $UnsetVars
        }
        # Ensure the environment variables are set
        $UnsetVars = Get-UnsetRequiredEnvironmentVariable @('TENANT_ID', 'CLIENT_ID', 'CLIENT_SECRET', 'CLIENT_CERTIFICATE', 'CLIENT_CERTIFICATE_PRIVATE_KEY')

        if ($UnsetVars.Count -gt 0) {
            throw "Environment variable(s) must be set: $($UnsetVars -join ', ')"
        }
    }

    # Tests is an array of hashtables with keys 'Splat', 'ExpectedScope', 'Flow', and 'Resource'
    Context 'for flow <Flow>' -ForEach $NonInteractiveTestCases {
        BeforeAll {
            $TokenHash = @{
                Token = $null
                Now   = $null
            }
        }
        BeforeEach {
            # Ensure the environment variables are set
            $UnsetVars = Get-UnsetRequiredEnvironmentVariable @('TENANT_ID', 'CLIENT_ID', 'CLIENT_SECRET', 'CLIENT_CERTIFICATE', 'CLIENT_CERTIFICATE_PRIVATE_KEY')

            if ($UnsetVars.Count -gt 0) {
                Set-ItResult -Inconclusive -Because "Environment variable(s) not set.)"
            }
        }

        It 'gets a token for <Resource>' {
            {
                # Save the token, the current time, and the username
                $TokenHash['Token'] = Get-AzToken @Splat
                $TokenHash['Now'] = ([System.DateTimeOffset]::Now)
            } | Should -Not -Throw

            $TokenHash['Token'] | Should -Not -BeNullOrEmpty
            $TokenHash['Token'].Token | Should -BeOfType [string]
        }

        It 'is for the correct identity' {
            $TokenHash['Token'].Claims['appid'] | Should -Be $env:CLIENT_ID
            # Identity is the object id of the service principal
            $TokenHash['Token'].Identity -as [guid] | Should -BeOfType [guid]
        }

        It 'has the correct scope' {
            $TokenHash['Token'].Scopes | Should -Contain $ExpectedScope
        }

        It 'has the correct resource' {
            $TokenHash['Token'].Claims['aud'] | Should -Be $Splat['Resource']
        }

        It 'is valid' {
            $TokenHash['Now'] -lt $TokenHash['Token'].ExpiresOn | Should -BeTrue
        }

        It 'has claims' {
            $TokenHash['Token'].Claims | Should -Not -BeNullOrEmpty
            $TokenHash['Token'].Claims.Count | Should -BeGreaterThan 1
        }

        AfterAll {
            # If a token cache was created and used, clear it after the test
            if ($Splat.ContainsKey('TokenCache') -and $Splat.ContainsKey('Username')) {
                $script:CachesToRemove += $Splat['TokenCache']
            }

            # Clean up the token hash variable
            Remove-Variable -Name TokenHash -ErrorAction SilentlyContinue
        }
    }
}