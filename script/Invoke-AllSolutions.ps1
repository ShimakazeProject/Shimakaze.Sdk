#Requires -Version 7.2

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
    $SlnRoot = Join-Path $ProjectRoot 'sln'

    $env:DOTNET_CLI_UI_LANGUAGE = "en_US"
  }

  process {
    $result = Get-ChildItem -Path $SlnRoot/*.sln `
    | ForEach-Object {
      $Name = $PSItem.Name.Replace('.Tests', '')
      $Display = $Echo -f $Name
      Write-Host -ForegroundColor Blue $Display
      if ($Parallel) {
        Start-Job -ScriptBlock $Action -ArgumentList $PSItem.FullName -WorkingDirectory $ProjectRoot
      }
      else {
        Invoke-Command -ScriptBlock $Action -ArgumentList $PSItem.FullName | Receive-PSSession
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