task setupALC {
    $ProjectName = Get-SamplerProjectName -BuildRoot $BuildRoot
    $Path = (Get-Module -ListAvailable "$BuildModuleOutput/$ProjectName" | Sort-Object Version -Descending | Select-Object -First 1).Path

    $ModuleFolder = Split-Path -Path (Resolve-Path -Path $Path) -Parent
    Write-Host $Path
    Write-Host $ModuleFolder

    Push-Location $ModuleFolder

    Rename-Item -Path 'AzAuth.Core' -NewName 'dependencies'
    Get-ChildItem -Path 'AzAuth.Net' -File | Move-Item -Destination .
    Get-ChildItem -Path 'AzAuth.PS' -File -Filter AzAuth.PS* | Move-Item -Destination .
    
    Remove-Item AzAuth.Net
    Remove-Item AzAuth.PS

    Pop-Location
}