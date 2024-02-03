import * as extension from '@shimakaze.sdk/extension'
import * as dotnet from '@shimakaze.sdk/extension/utils/ipc/dotnet'
import * as jsonrpc from '@shimakaze.sdk/extension/utils/ipc/jsonrpc'
import * as logger from '@shimakaze.sdk/extension/utils/logger'
import * as cp from 'node:child_process'
import * as path from 'node:path'
import * as vscode from 'vscode'

/** .Net 程序集路径 */
const getAssemblyPath = (extensionPath: string) =>
  path.join(extensionPath, 'bin', 'Shimakaze.Sdk.ShpViewer.dll')

let _process: cp.ChildProcessWithoutNullStreams | undefined = undefined
let _connection: ReturnType<typeof jsonrpc.connect> | undefined = undefined

/**
 * 启动.Net进程
 * @param extension 扩展实例
 */
const runProcess = async (extension: extension.MyExtension) =>
  (_process ??= await (async () => {
    const dotnetPath = dotnet.getDotnetPath(extension.id)
    const assemblyPath = getAssemblyPath(extension.extensionPath)
    const args = [assemblyPath]

    // This will install any missing Linux dependencies.
    await vscode.commands.executeCommand('dotnet.ensureDotnetDependencies', {
      command: dotnetPath,
      arguments: args,
    })

    return cp.spawn(await dotnetPath, args)
  })())

export const current = () => _connection

export const getServer = async (context: vscode.ExtensionContext) =>
  (_connection ??= await (async () => {
    {
      const childProcess = await runProcess(context.extension)

      childProcess.stdout.addListener('data', data => {
        logger.getLogChannel().appendLine(data)
      })

      childProcess.on('close', dispose)
      context.subscriptions.push(
        new vscode.Disposable(() => {
          childProcess.kill()
        }),
      )

      const connection = jsonrpc.connect(
        childProcess.stdin,
        childProcess.stderr,
      )
      context.subscriptions.push(connection)

      return connection
    }
  })())

export const dispose = () => {
  _connection?.dispose()
  _connection = undefined
}
