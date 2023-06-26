#Requires -Version 7.3


function Invoke-AllProject {
  [CmdletBinding()]
  param (
    [scriptblock]
    $Action,
    [switch]
    $Parallel,
    [string]
    $Echo
  )

  begin {
    $ProjectRoot = Join-Path $PSScriptRoot '..'
    $SrcRoot = Join-Path $ProjectRoot 'src'
    $TestRoot = Join-Path $ProjectRoot 'test'

    $env:DOTNET_CLI_UI_LANGUAGE = "en_US"
  }

  process {
    $result = Get-ChildItem $SrcRoot `
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
      $Display = $Echo -f $Name
      Write-Host -ForegroundColor Blue $Display
      if ($Parallel) {
        Start-Job -ScriptBlock $Action -ArgumentList $PSItem.FullName -WorkingDirectory $ProjectRoot
      }
      else {
        Invoke-Command -ScriptBlock $Action -ArgumentList $PSItem.FullName
      }
    }
  }

  end {
    if ($Parallel) {
      $result | Wait-Job | Remove-Job | Out-Null
    }
    Write-Host -ForegroundColor Green 'All Done !'
  }
}