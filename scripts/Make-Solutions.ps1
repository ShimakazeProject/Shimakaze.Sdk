#Requires -Version 7.3

$ProjectRoot = Join-Path $PSScriptRoot '..'
$TestRoot = Join-Path $ProjectRoot 'test'

Get-ChildItem $TestRoot | ForEach-Object {
  $Name = $_.Name
  $MSBuildProjectPath = Join-Path $TestRoot $Name "$Name.csproj"
  if (Test-Path $MSBuildProjectPath) {
    dotnet build """$MSBuildProjectPath""" -t:SlnGen -p:SlnGenLaunchVisualStudio=false
  }
}
