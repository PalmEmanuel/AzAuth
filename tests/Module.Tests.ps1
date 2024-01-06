BeforeDiscovery {
    $ModuleName = Get-SamplerProjectName -BuildRoot $BuildRoot

    # Get exported commands
    $ExportedCommands = (Get-Module $ModuleName).ExportedCommands.Keys

    # Set up testcases
    $CommandTestCases = @()
    $ParametersTestCases = @()
    # Get custom parameters of all exported commands
    foreach ($Command in $ExportedCommands) {
        $Parameters = (Get-Command $Command).Parameters.GetEnumerator() | Where-Object {
            $_.Key -notin [System.Management.Automation.Cmdlet]::CommonParameters -and
            $_.Value.Attributes.DontShow -eq $false
        } | Select-Object -ExpandProperty Key

        foreach ($Parameter in $Parameters) {
            $ParametersTestCases += @{
                Command   = $Command
                Parameter = $Parameter
            }
        }

        $CommandTestCases += @{
            Command = $Command
        }
    }
}

BeforeAll {
    $ModuleName = Get-SamplerProjectName -BuildRoot $BuildRoot
}

Describe "$ModuleName" {
    # A module should always have exported commands
    Context 'module' {
        # Tests run on both uncompiled and compiled modules
        It 'has commands' -TestCases (@{ Count = $CommandTestCases.Count }) {
            $Count | Should -BeGreaterThan 0 -Because 'commands should exist'
        }
        
        It 'has no help file with empty documentation sections' {
            Get-ChildItem "$BuildRoot\docs\help\*.md" | Select-String '{{|}}' | Should -BeNullOrEmpty
        }
        
        It 'has command <Command> defined in file in the correct directory' -TestCases $CommandTestCases {
            $CommandFileName = $Command -replace '-'
            
            "$BuildRoot\source\$ModuleName.PS\Cmdlets\$CommandFileName.cs" | Should -Exist
        }

        It 'has test file for command <Command>' -TestCases $CommandTestCases {
            $Command
            
            "$BuildRoot\tests\$Command.Tests.ps1" | Should -Exist
        }

        It 'has markdown help file for command <Command>' -TestCases $CommandTestCases {
            "$BuildRoot\docs\help\$Command.md" | Should -Exist
        }

        It 'has parameter <Parameter> documented in markdown help file for command <Command>' -TestCases $ParametersTestCases {
            "$BuildRoot\docs\help\$Command.md" | Should -FileContentMatch $Parameter
        }
    }
}