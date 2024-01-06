task setupALC {
    $ProjectName = Get-SamplerProjectName -BuildRoot $BuildRoot
    $Path = (Get-Module -ListAvailable "$BuildModuleOutput/$ProjectName" | Sort-Object Version -Descending | Select-Object -First 1).Path

    $ModuleFolder = Split-Path -Path (Resolve-Path -Path $Path) -Parent
    Write-Host $Path
    Write-Host $ModuleFolder

    Push-Location $ModuleFolder

    Rename-Item -Path 'AzAuth.Core' -NewName 'dependencies'
    Get-ChildItem -Path 'AzAuth.PS' -File -Filter AzAuth.PS* | Move-Item -Destination .
    
    # We need to load the DLLs for logging and JoinableTaskFactory in the PS context, not in the assembly load context
    Move-Item -Path @(
        "./dependencies/Microsoft.VisualStudio.Threading.dll"
        "./dependencies/Microsoft.VisualStudio.Validation.dll"
    ) -Destination .
    
    Remove-Item AzAuth.PS

    Pop-Location
}