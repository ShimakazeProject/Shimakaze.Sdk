import * as vscode from 'vscode'
import { initCsfLabelView } from './views/csf/label'
import { initCsfValueView } from './views/csf/value'

export async function activate (context: vscode.ExtensionContext) {
  const logChannel = vscode.window.createOutputChannel('Shimakaze SDK Log')
  initCsfLabelView(context, logChannel)
  initCsfValueView(context, logChannel)

  // context.subscriptions.push(vscode.commands.registerCommand('shimakaze-sdk-vscode.csf.editor', () => {}))
}

export function deactivate () { }
