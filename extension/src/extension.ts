import * as vscode from 'vscode'
import { CsfEditViewPanel } from './views/CsfEditView'
import { CsfEditViewProvider } from './views/CsfEditView/new'
import { CsfFileProvider } from './views/CsfFileTreeView'

export async function activate (context: vscode.ExtensionContext) {
  const helloCommand = vscode.commands.registerCommand('shimakaze-sdk-vscode.csf.editor', () => {
    CsfEditViewPanel.render(context.extensionUri)
  })

  context.subscriptions.push(helloCommand)

  const csfEditPanelProvider = new CsfEditViewProvider(context.extensionUri)

  context.subscriptions.push(
    vscode.window.registerWebviewViewProvider(CsfEditViewProvider.viewType, csfEditPanelProvider)
  )
  context.subscriptions.push(
    vscode.window.registerTreeDataProvider(CsfFileProvider.viewType, new CsfFileProvider())
  )
}

export function deactivate () { }
