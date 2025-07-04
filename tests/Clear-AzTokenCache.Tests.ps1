BeforeDiscovery {
    $ParameterTestCases += @(
        @{
            Name          = 'TokenCache'
            Type          = 'string'
        },
        @{
            Name          = 'Force'
            Type          = 'switch'
        },
        @{
            Name          = 'RootPath'
            Type          = 'string'
        }
    )
}

Describe 'Clear-AzTokenCache' {
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
    }
}