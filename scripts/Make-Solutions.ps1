#Requires -Version 7.3

$ProjectRoot = Join-Path $PSScriptRoot '..'
$TestRoot = Join-Path $ProjectRoot 'test'

Get-ChildItem $TestRoot `
| Where-Object {
  $PSItem -is [System.IO.DirectoryInfo]
} `
| ForEach-Object {
  Write-Output "Generate $($PSItem.Name.Replace('.Tests', ''))"
  dotnet build $PSItem.FullName -t:SlnGen -p:SlnGenLaunchVisualStudio=False
}