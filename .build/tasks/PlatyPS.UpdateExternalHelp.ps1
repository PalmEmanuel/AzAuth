task updateExternalHelp {
    $ProjectName = Get-SamplerProjectName -BuildRoot $BuildRoot
    Import-Module 'platyPS' -ErrorAction 'Stop'
    $ModuleManifestPath = (Get-Module -ListAvailable "$BuildModuleOutput/$ProjectName" | Sort-Object Version -Descending | Select-Object -First 1).Path
    $ModuleVersionPath = Split-Path -Path $ModuleManifestPath -Parent
    New-ExternalHelp "$BuildRoot/$HelpSourceFolder/$HelpOutputFolder" -OutputPath "$ModuleVersionPath/$HelpCultureInfo" -Force
}