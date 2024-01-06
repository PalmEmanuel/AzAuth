task updateMarkdownHelp {
    Import-Module 'platyPS' -ErrorAction 'Stop'
    $ProjectName = Get-SamplerProjectName -BuildRoot $BuildRoot
    Import-Module "$BuildModuleOutput/$ProjectName" -Force -ErrorAction 'Stop'
    Update-MarkdownHelpModule -Path "$BuildRoot/$HelpSourceFolder/$HelpOutputFolder"
}