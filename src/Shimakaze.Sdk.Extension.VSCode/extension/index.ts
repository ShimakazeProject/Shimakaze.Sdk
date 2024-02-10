import * as vscode from 'vscode'

export type MyExtension = vscode.Extension<ReturnType<typeof activate>>

export function activate(context: vscode.ExtensionContext) {
  if (import.meta.env.DEV) {
    vscode.window.showInformationMessage(
      `Shimakaze.Sdk for VSCode 已被激活 ${context.extension.id}`,
    )
  }
}

export function deactivate() {}
