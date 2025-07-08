BeforeAll {
    $script:CachesToRemove = @()
}

Describe 'Get-AzToken interactive integration tests' -Tag 'Interactive', 'Integration' {
    BeforeDiscovery {

        . "$BuildRoot\tests\cases\TestCases.ps1"
        $InteractiveTestCases = Get-TestCaseByType -CaseType 'InteractiveTests'
    }
    
    BeforeAll {
        # These tests require user interaction and should only be run manually
        Write-Host @'
These tests require user interaction and will prompt for authentication.
The tokens acquired in the integration tests are real, and should be handled as secrets!
'@
        # Create a variable to hold the last username used
        # This allows us to save the username used in interactive tests
        # and use it for cache tests (silent login)
        $script:LastUsername = $null
    }

    # Tests is an array of hashtables with keys 'Splat', 'ExpectedScope', 'Flow', and 'Resource'
    Context 'for flow <Flow>' -ForEach $InteractiveTestCases {
        BeforeAll {
            $TokenHash = @{
                Token = $null
                Now   = $null
            }
        }

        It 'gets a token for <Resource>' {
            {
                # If the splat contains a username, it's a cache test
                # In that case, set it to the last username used
                if ($Splat.ContainsKey('Username')) {
                    $Splat['Username'] = $script:LastUsername
                }

                # Save the token, the current time, and the username
                $TokenHash['Token'] = Get-AzToken @Splat
                $TokenHash['Now'] = ([System.DateTimeOffset]::Now)
                $script:LastUsername = $TokenHash['Token'].Identity
            } | Should -Not -Throw

            $TokenHash['Token'] | Should -Not -BeNullOrEmpty
            $TokenHash['Token'].Token | Should -BeOfType [string]
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

AfterAll {
    foreach ($Cache in $script:CachesToRemove) {
        Clear-AzTokenCache -TokenCache $Cache -Force
    }
    Remove-Variable -Name CachesToRemove -ErrorAction SilentlyContinue
}