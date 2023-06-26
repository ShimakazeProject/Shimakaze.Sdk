#Requires -Version 7.3

$ProjectRoot = Join-Path $PSScriptRoot '..'
$SrcRoot = Join-Path $ProjectRoot 'src'
$TestRoot = Join-Path $ProjectRoot 'test'
$SlnRoot = Join-Path $ProjectRoot 'sln'

Write-Host -ForegroundColor Green '##########################'
Write-Host -ForegroundColor Green '#                        #'
Write-Host -ForegroundColor Green '#  Solution Generator    #'
Write-Host -ForegroundColor Green '#            -- frg2089  #'
Write-Host -ForegroundColor Green '#                        #'
Write-Host -ForegroundColor Green '##########################'

$env:DOTNET_CLI_UI_LANGUAGE = "en_US"

Get-ChildItem -Path (Join-Path $SlnRoot '*.sln') | Remove-Item

Get-ChildItem $SrcRoot `
| Where-Object {
  $PSItem -is [System.IO.DirectoryInfo]
} `
| ForEach-Object {
  $TestProject = Join-Path $TestRoot "$($PSItem.Name).Tests"
  if (Test-Path $TestProject) {
    Get-Item $TestProject
    return
  }

  $PSItem
} `
| ForEach-Object {
  $Name = $PSItem.Name.Replace('.Tests', '')
  Write-Host -ForegroundColor Blue "Generating ""$Name.sln"" ..."
  Start-Job -ScriptBlock {
    dotnet build $input -t:SlnGen -p:SlnGenLaunchVisualStudio=False
  } -InputObject $PSItem.FullName
} | Wait-Job | Remove-Job | Out-Null
Write-Host -ForegroundColor Green 'All Done !'
