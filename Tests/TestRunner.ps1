param (
    [string]$ModuleLoadPath = (Get-ChildItem "$PSScriptRoot\..\*\*.psd1"),
    
    [string[]]$TestsPath = $PSScriptRoot,
    
    [string]$Verbosity = 'Detailed',

    [string[]]$CodeCoveragePath,

    [switch]$TestResults,

    [switch]$PassThru
)

$AllTests = Get-ChildItem -Path $TestsPath -Filter *.Tests.ps1 | Select-Object -ExpandProperty FullName

$PesterConfiguration = New-PesterConfiguration
$PesterConfiguration.Output.Verbosity = $Verbosity

if ($PassThru.IsPresent) {
    $PesterConfiguration.Run.PassThru = $True
}

$ModuleLoadData = @{
    Manifest = Get-Item $ModuleLoadPath | Select-Object -ExpandProperty FullName
}

$Container = New-PesterContainer -Path $AllTests -Data $ModuleLoadData
$PesterConfiguration.Run.Container = $Container

if ($CodeCoveragePath) {      
    $AllCodeCoverageFiles = Get-ChildItem -Path $CodeCoveragePath | Select-Object -ExpandProperty FullName

    $PesterConfiguration.CodeCoverage.Enabled = $true
    $PesterConfiguration.CodeCoverage.Path = $AllCodeCoverageFiles
    $PesterConfiguration.CodeCoverage.CoveragePercentTarget = 75
    $PesterConfiguration.CodeCoverage.OutputPath = './coverage.xml'
    $PesterConfiguration.CodeCoverage.OutputFormat = 'CoverageGutters'
}

if ($TestResults.IsPresent) {
    $PesterConfiguration.TestResult.Enabled = $true
    $PesterConfiguration.TestResult.OutputFormat = 'JUnitXml'
}

Invoke-Pester -Configuration $PesterConfiguration