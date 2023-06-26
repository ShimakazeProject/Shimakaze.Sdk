#Requires -Version 7.2

[CmdletBinding()]
param (
    [Parameter()]
    [switch]
    $NoParallel
)

. $PSScriptRoot/Invoke-AllProject.ps1

$ProjectRoot = Join-Path $PSScriptRoot '..'
$SlnRoot = Join-Path $ProjectRoot 'sln'

Write-Host -ForegroundColor Green '##########################'
Write-Host -ForegroundColor Green '#                        #'
Write-Host -ForegroundColor Green '#  Solution Generator    #'
Write-Host -ForegroundColor Green '#            -- frg2089  #'
Write-Host -ForegroundColor Green '#                        #'
Write-Host -ForegroundColor Green '##########################'

Get-ChildItem -Path (Join-Path $SlnRoot '*.sln') | Remove-Item

Invoke-AllProject {
  dotnet build $args -t:SlnGen -p:SlnGenLaunchVisualStudio=False
} -Parallel:(!$NoParallel) -Echo "Generating ""{0}.sln"""