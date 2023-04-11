function Invoke-ModuleReload {
    param ($Manifest)
    
    Remove-Module (Get-Module $Manifest -ListAvailable).Name -Force -ErrorAction SilentlyContinue
    Import-Module $Manifest -Force
}