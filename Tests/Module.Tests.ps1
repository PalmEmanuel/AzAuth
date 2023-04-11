param(
    [Parameter()]
    [ValidateScript({ $_ -match '\.psd1$' }, ErrorMessage = 'Please input a .psd1 file')]
    $Manifest
)

BeforeDiscovery {
    # Get module name from manifest
    $ModuleName = (Get-Module $Manifest -ListAvailable).Name
    
    # Remove and import module
    . "$PSScriptRoot\CommonTestLogic.ps1"
    Invoke-ModuleReload -Manifest $Manifest

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
    $RootDirectory = "$(Split-Path -Path $Manifest -Parent)\..\"
    $ModuleName = (Get-Module $Manifest -ListAvailable).Name
}

Describe "$ModuleName" {
    # A module should always have exported commands
    Context 'module' {
        # Tests run on both uncompiled and compiled modules
        It 'has commands' -TestCases (@{ Count = $CommandTestCases.Count }) {
            $Count | Should -BeGreaterThan 0 -Because 'commands should exist'
        }
        
        It 'has no help file with empty documentation sections' {
            Get-ChildItem "$RootDirectory\Docs\Help\*.md" | Select-String '{{|}}' | Should -BeNullOrEmpty
        }
        
        It 'has command <Command> defined in file in the correct directory' -TestCases $CommandTestCases {
            $CommandFileName = $Command -replace '-'
            
            "$RootDirectory\Source\$ModuleName.PS\Cmdlets\$CommandFileName.cs" | Should -Exist
        }

        It 'has test file for command <Command>' -TestCases $CommandTestCases {
            $Command
            
            "$RootDirectory\Tests\$Command.Tests.ps1" | Should -Exist
        }

        It 'has markdown help file for command <Command>' -TestCases $CommandTestCases {
            "$RootDirectory\Docs\Help\$Command.md" | Should -Exist
        }

        It 'has parameter <Parameter> documented in markdown help file for command <Command>' -TestCases $ParametersTestCases {
            "$RootDirectory\Docs\Help\$Command.md" | Should -FileContentMatch $Parameter
        }
    }
}