param(
    [string[]]
    # Naming this $ProjectPath would cause it to overwrite the variable passed to Pester
    $Paths = @('source/AzAuth.Core', 'source/AzAuth.PS'),

    [ValidateSet('Debug', 'Release')]
    [string]
    $Configuration = (property CompileConfiguration 'Release')
)

task dotnetBuild {
    $CommonFiles = [System.Collections.Generic.HashSet[string]]::new()

    foreach ($Path in $Paths) {
        $OutPathFolder = Split-Path -Path (Resolve-Path -Path $Path) -Leaf
        Write-Host $Path
        Write-Host $OutPathFolder
        $OutPath = "bin/$OutPathFolder"
        if (-not (Test-Path -Path $Path)) {
            throw "Path '$Path' does not exist."
        }
        
        Push-Location -Path $path
        
        # Remove output folder if exists
        if (Test-Path -Path $OutPath) {
            Remove-Item -Path $OutPath -Recurse -Force
        }

        Write-Host "Building '$Path' to '$OutPath'" -ForegroundColor 'Magenta'
        dotnet publish -c $Configuration -o $OutPath

        # Remove everything we don't need from the build
        Get-ChildItem -Path $OutPath |
            Foreach-Object {
                if ($_.Extension -notin '.dll', '.pdb' -or $CommonFiles.Contains($_.Name)) {
                    # Only keep DLLs and PDBs, and only keep one copy of each file.
                    Remove-Item $_.FullName -Recurse -Force
                }
                else {
                    [void]$CommonFiles.Add($_.Name)
                }
            }

        Pop-Location
    }
}