import * as vscode from 'vscode'
import * as commands from '@shimakaze.sdk/extension/commands'
import * as daemon from '@shimakaze.sdk/extension/utils/ipc/daemon'
import * as logger from '@shimakaze.sdk/extension/utils/logger'

export type MyExtension = vscode.Extension<ReturnType<typeof activate>>

export function activate(context: vscode.ExtensionContext) {
  vscode.window.showInformationMessage(
    `Shimakaze.Sdk for VSCode 已被激活 ${context.extension.id}`,
  )
  commands.shp.view.registry(context)
}

export function deactivate() {
  daemon.dispose()
  logger.dispose()
}
