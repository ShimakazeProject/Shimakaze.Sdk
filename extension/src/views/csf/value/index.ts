import * as vscode from 'vscode'
import { _viewCommand } from '../../../utils/command'
import { put } from './commands'
import { CsfValueViewProvider } from './provider'

export const initCsfValueView = async (context: vscode.ExtensionContext, log: vscode.OutputChannel) => {
  const csfValueViewProvider = new CsfValueViewProvider(context.extensionUri)

  context.subscriptions.push(
    vscode.window.registerWebviewViewProvider(CsfValueViewProvider.viewType, csfValueViewProvider),
    vscode.commands.registerCommand(_viewCommand(CsfValueViewProvider.viewType, 'put'), put(csfValueViewProvider, log))
  )
}
