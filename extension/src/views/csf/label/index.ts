import * as vscode from 'vscode'
import { CsfLabelViewProvider } from './provider'
import { createCsf, removeCsf, savetoCsf, updateCsf } from './commands'
import { _viewCommand } from '../../../utils/command'
import { CsfValueViewProvider } from '../value/provider'
import { parseNodes } from './utils/parse'

const setVisibility = async (visible: boolean) => {
  vscode.commands.executeCommand('setContext', 'shimakaze-sdk:showCsfLabelView', visible)
  if (visible) {
    await vscode.commands.executeCommand(`${CsfLabelViewProvider.viewType}.focus`)
    await vscode.commands.executeCommand(`${CsfValueViewProvider.viewType}.focus`)
  }
}

const detectCsf = async (textEditor: vscode.TextEditor | undefined, provider: CsfLabelViewProvider, log: vscode.OutputChannel) => {
  const data = await parseNodes(textEditor, log)
  provider.data = data
  await setVisibility(!!provider.data)
}

export const initCsfLabelView = async (context: vscode.ExtensionContext, log: vscode.OutputChannel) => {
  const csfLabelViewProvider = new CsfLabelViewProvider()

  const treeView = vscode.window.createTreeView('shimakaze-sdk-activitybar', {
    treeDataProvider: csfLabelViewProvider,
    canSelectMany: false
  })

  vscode.window.onDidChangeActiveTextEditor(async textEditor => await detectCsf(textEditor, csfLabelViewProvider, log))

  context.subscriptions.push(
    vscode.window.registerTreeDataProvider(CsfLabelViewProvider.viewType, csfLabelViewProvider),
    vscode.commands.registerCommand(_viewCommand(CsfLabelViewProvider.viewType, 'create'), createCsf(treeView, csfLabelViewProvider, log)),
    vscode.commands.registerCommand(_viewCommand(CsfLabelViewProvider.viewType, 'update'), updateCsf(treeView, csfLabelViewProvider, log)),
    vscode.commands.registerCommand(_viewCommand(CsfLabelViewProvider.viewType, 'remove'), removeCsf(treeView, csfLabelViewProvider, log)),
    vscode.commands.registerCommand(_viewCommand(CsfLabelViewProvider.viewType, 'saveto'), savetoCsf(treeView, csfLabelViewProvider, log))
  )

  await detectCsf(vscode.window.activeTextEditor, csfLabelViewProvider, log)
}
