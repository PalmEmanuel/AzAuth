param(
    [ValidateSet('Debug', 'Release')]
    [string]
    $Configuration = 'Release',

    [string]
    $Version,

    [Switch]
    $NoClean
)

Push-Location 'Source'

$ModuleName = $PSScriptRoot.Split('\')[-1]
$Configuration = 'Release'
$DotNetVersion = 'net6.0'

# Define build output locations
$OutDir = "$PSScriptRoot\$ModuleName"
$OutDependencies = "$OutDir\dependencies"

if (Test-Path $OutDir) {
    Remove-Item $OutDir -Recurse -Force -ErrorAction Stop
}

# Build both Core and PS projects
if (-not $NoClean.IsPresent) {
    dotnet build-server shutdown
    dotnet clean -c $Configuration
}
dotnet publish -c $Configuration

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

Copy-Item -Path "$ModuleName.PS\Manifest\$ModuleName.psd1" -Destination $OutDir
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