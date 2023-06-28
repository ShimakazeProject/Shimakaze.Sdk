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
    
    $env:DOTNET_CLI_UI_LANGUAGE = 'en'
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
        $Job = Start-Job -ScriptBlock $Action -ArgumentList $PSItem.FullName -WorkingDirectory $ProjectRoot | Wait-Job
        # 使用 Receive-Job 获取作业的输出，其中包含 dotnet build 的退出代码
        $ExitCode = Receive-Job $Job

        # 检查退出代码
        if ($ExitCode -ne 0) {
          Write-Error "Failed"
          Remove-Item Env:\DOTNET_CLI_UI_LANGUAGE
          break
        }
        $Job | Remove-Job | Out-Null
      }
    }
  }

  end {
    if ($Parallel) {
      $result | Wait-Job | Remove-Job | Out-Null
    }
    Write-Host -ForegroundColor Green 'All Done !'
    Remove-Item Env:\DOTNET_CLI_UI_LANGUAGE
  }
}