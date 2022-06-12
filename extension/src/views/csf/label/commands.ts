import * as vscode from 'vscode'
import type { CsfUnit } from '../../../@types/CsfModel'
import { format as formatV2 } from '../../../utils/csfv2'
import { CsfNode } from './CsfNode'
import { CsfLabelViewProvider } from './provider'

/**
 * 创建一个标签
 * @param treeview 树视图
 * @param provider provider
 * @returns 命令
 */
export const createCsf = (treeview: vscode.TreeView<CsfNode>, provider: CsfLabelViewProvider, log: vscode.OutputChannel) => async () => {
  const type = 'New'
  const newData: CsfUnit = {
    label: `${type}:Label`,
    values: [
      { value: 'New Value' }
    ]
  }
  // 通过数据创建节点
  const node = CsfNode.create(newData)
  // 查找类型节点
  let typeNode = provider.data?.find(node => node.label === type)

  // 如果类型节点不存在就新建一个
  if (!typeNode) {
    typeNode = CsfNode.create(type, [node])
    provider.data?.push(typeNode)
  } else {
    typeNode.children?.push(node)
  }

  provider.refresh()
  await treeview.reveal(node, { select: true, focus: true })
}

/**
 * 更新标签的数据
 * @param treeview 树视图
 * @param provider provider
 * @returns 命令
 */
export const updateCsf = (treeview: vscode.TreeView<CsfNode>, provider: CsfLabelViewProvider, log: vscode.OutputChannel) => async (node: CsfNode, newData: CsfUnit, ...args: any[]) => {
  node.data = newData

  await treeview.reveal(node, { focus: true })
}

/**
 * 删除标签
 * @param treeview 树视图
 * @param provider provider
 * @returns 命令
 */
export const removeCsf = (treeview: vscode.TreeView<CsfNode>, provider: CsfLabelViewProvider, log: vscode.OutputChannel) => async (node: CsfNode, ...args: any[]) => {
  if (node.parent && node.parent.children) {
    // 从父节点中删除
    const index = node.parent.children.indexOf(node)
    if (index > 0) {
      node.parent.children.splice(index, 1)
      provider.refresh()
      await treeview.reveal(node.parent, { focus: true })
      return
    }
  } else if (provider.data) {
    // 删除整个Type节点
    const index = provider.data.indexOf(node)
    if (index > 0) {
      provider.data?.splice(index, 1)
      provider.refresh()
      return
    }
  }

  log.appendLine(`删除失败：${node.label}`)
}

/**
 * 保存
 * @param treeview 树视图
 * @param provider provider
 * @returns 命令
 */
export const savetoCsf = (treeview: vscode.TreeView<CsfNode>, provider: CsfLabelViewProvider, log: vscode.OutputChannel) => () => {
  if (!provider.data) {
    return
  }
  const obj = formatV2(provider.data.flatMap(i => i.children!.flatMap(v => v.data!)))
  const content = JSON.stringify(obj, undefined, 2)
  const textEditor = vscode.window.activeTextEditor
  textEditor?.edit(editBuilder => {
    editBuilder.replace(
      new vscode.Range(
        0,
        0,
        textEditor?.document.lineCount,
        textEditor?.document.lineAt(textEditor?.document.lineCount - 1).rangeIncludingLineBreak.end.character),
      content
    )
  })
}
