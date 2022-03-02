param(
    [ValidateSet('Debug', 'Release')]
    [string]
    $Configuration = 'Release',

    [string]
    $Version,

    [Switch]
    $Full
)

$ModuleName = 'AzAuth'
$DotNetVersion = 'net6.0'

$ProjectRoot = $PSScriptRoot
$ManifestDirectory = "$ProjectRoot/$ModuleName"
$ModuleDirectory = "$ManifestDirectory/$ModuleName"

if (Test-Path $ManifestDirectory) {
    Remove-Item -Path $ManifestDirectory -Recurse
}
$null = New-Item -Path $ManifestDirectory -ItemType Directory
$null = New-Item -Path $ModuleDirectory -ItemType Directory

if ($Full) {
    dotnet build-server shutdown
    dotnet clean '.\Source'
}
dotnet publish ".\Source" -c $Configuration

$ModuleFiles = [System.Collections.Generic.HashSet[string]]::new()

Get-ChildItem -Path "$ProjectRoot/Source/bin/$Configuration/$DotNetVersion/publish" |
Where-Object { $_.Extension -in '.dll', '.pdb' } |
ForEach-Object { 
    [void]$ModuleFiles.Add($_.Name); 
    Copy-Item -LiteralPath $_.FullName -Destination $ModuleDirectory 
}

Copy-Item -Path "$ProjectRoot/Source/Manifest/$ModuleName.psd1" -Destination $ManifestDirectory
if (-not $PSBoundParameters.ContainsKey('Version')) {
    try {
        $Version = gitversion /showvariable LegacySemVerPadded
    }
    catch {
        $Version = [string]::Empty
    }
}
if($Version) {
    $SemVer, $PreReleaseTag = $Version.Split('-')
    Update-ModuleManifest -Path "$ManifestDirectory/$ModuleName.psd1" -ModuleVersion $SemVer -Prerelease $PreReleaseTag
}

Compress-Archive -Path $ManifestDirectory -DestinationPath "$ProjectRoot/$ModuleName.zip" -Force