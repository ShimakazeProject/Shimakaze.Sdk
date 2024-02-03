import { cmdPrefix } from '@shimakaze.sdk/extension/commands'
import { ShpViewer } from '@shimakaze.sdk/extension/panels/ShpViewer'
import * as utils from '@shimakaze.sdk/extension/utils'
import * as path from 'node:path'
import * as vscode from 'vscode'

interface UrlObject {
  uri: vscode.Uri
}

export const canExecute = async () => {
  // 检查是否是受信任的工作区
  if (!vscode.workspace.isTrusted)
    throw new Error(vscode.l10n.t('活动的工作区不受信任'))

  // 当前文件
  const file = (
    vscode.window.tabGroups.activeTabGroup.activeTab?.input as
      | Partial<UrlObject>
      | undefined
  )?.uri

  // 找不到活动的文件
  if (!file) throw new Error(vscode.l10n.t('找不到活动的文件'))

  if (path.extname(file.fsPath).toLowerCase() !== '.shp')
    throw new Error(vscode.l10n.t('文件不是一个有效的SHP文件'))

  // 根据当前打开的文件找工作区
  const folder = vscode.workspace.getWorkspaceFolder(file)?.uri

  // 不在工作区内
  if (!folder) throw new Error(vscode.l10n.t('找不到活动的工作区'))

  return {
    file,
    folder,
  }
}

export const execute = (context: vscode.ExtensionContext) => async () => {
  const { file, folder } = await canExecute()
  const cacheFolder = utils.getCacheFolder(folder, 'shp', '.cache')

  const viewer = new ShpViewer(context, cacheFolder, file)
  context.subscriptions.push(viewer)
}

export const registry = (context: vscode.ExtensionContext) => {
  context.subscriptions.push(
    vscode.commands.registerCommand(`${cmdPrefix}shp/view`, execute(context)),
  )
}
