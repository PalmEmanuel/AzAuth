param(
    [ValidateSet('Debug', 'Release')]
    [string]
    $Configuration = 'Release',
    
    [switch]
    $NoDebugSymbols,

    [string]
    $Version,
    
    [string]
    $DotNetVersion = 'netstandard2.0',

    [Switch]
    $NoClean,

    [ValidateSet('All','None')]
    $RunTests = 'All'
)

Push-Location 'Source'

$ModuleName = Get-ChildItem -Recurse "$PSScriptRoot\Source\*.psd1" | Select-Object -ExpandProperty BaseName

# Define build output locations
$OutDir = "$PSScriptRoot\$ModuleName"
$OutDependencies = "$OutDir\dependencies"
$OutDocs = "$OutDir/en-US"

if (Test-Path $OutDir) {
    Remove-Item $OutDir -Recurse -Force -ErrorAction Stop
}

# Build both Core and PS projects
if (-not $NoClean.IsPresent) {
    dotnet build-server shutdown
    dotnet clean -c $Configuration
}
if ($NoDebugSymbols.IsPresent) {
    dotnet publish -c $Configuration /p:DebugType=None /p:DebugSymbols=false
} else {
    dotnet publish -c $Configuration
}

# Ensure output directories exist and are clean for build
New-Item -Path $OutDir -ItemType Directory -ErrorAction Ignore
Get-ChildItem $OutDir | Remove-Item -Recurse
New-Item -Path $OutDependencies -ItemType Directory

# Create array to remember copied files
$CopiedDependencies = @()

# Copy .dll and .pdb files from Core to the dependency directory
Get-ChildItem -Path "$ModuleName.Core\bin\$Configuration\$DotNetVersion\publish" |
Where-Object { $_.Extension -in '.dll', '.pdb' } |
ForEach-Object {
    $CopiedDependencies += $_.Name
    Copy-Item -Path $_.FullName -Destination $OutDependencies
}

# Copy files from PS to output directory, except those already copied from Core
Get-ChildItem -Path "$ModuleName.PS\bin\$Configuration\$DotNetVersion\publish" |
Where-Object { $_.Name -notin $CopiedDependencies -and $_.Extension -in '.dll', '.pdb' } |
ForEach-Object {
    Copy-Item -Path $_.FullName -Destination $OutDir
}

# Copy manifest
Copy-Item -Path "$ModuleName.PS\Manifest\$ModuleName.psd1" -Destination $OutDir

# We need to load the DLLs for logging and JoinableTaskFactory in the PS context, not in the assembly load context
Move-Item -Path @(
    "$OutDependencies/Microsoft.VisualStudio.Threading.dll"
    "$OutDependencies/Microsoft.VisualStudio.Validation.dll"
)  -Destination $OutDir

if (-not $PSBoundParameters.ContainsKey('Version')) {
    try {
        $Version = gitversion /showvariable LegacySemVerPadded
    }
    catch {
        $Version = [string]::Empty
    }
}
if ($Version) {
    $SemVer, $PreReleaseTag = $Version.Split('-')
    Update-ModuleManifest -Path "$ManifestDirectory/$ModuleName.psd1" -ModuleVersion $SemVer -Prerelease $PreReleaseTag
}

Pop-Location

# Run markdown file updates and tests in separate PowerShell sessions to avoid module load assembly locking
& pwsh -c "Import-Module '$OutDir\$ModuleName.psd1'; Update-MarkdownHelpModule -Path '$PSScriptRoot\Docs\Help'"

# Workaround to run post-build to avoid platyPS generating documentation for common parameter ProgressAction
. "$PSScriptRoot\PlatyPSWorkaround.ps1"
Repair-PlatyPSMarkdown -Path (Get-ChildItem "$PSScriptRoot\Docs\Help") -ParameterName 'ProgressAction'

if ($RunTests -ne 'None') {
    & pwsh -c ".\Tests\TestRunner.ps1"
}

New-ExternalHelp -Path "$PSScriptRoot\Docs\Help" -OutputPath $OutDocs