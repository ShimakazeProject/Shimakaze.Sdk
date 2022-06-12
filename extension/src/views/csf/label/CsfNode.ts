import * as vscode from 'vscode'
import type { CsfUnit } from '../../../@types/CsfModel'
import { CsfValueViewProvider } from '../value/provider'
import { _viewCommand } from '../../../utils/command'

/**
   * 获取是否应该折叠
   * @param arg CsfUnit | CsfNode[]
   * @returns
   */
const getCollapsibleState = (arg: CsfNode[] | CsfUnit) : vscode.TreeItemCollapsibleState => {
  if (!(arg instanceof Array) || arg.length === 0) {
    return vscode.TreeItemCollapsibleState.None
  }
  return vscode.TreeItemCollapsibleState.Collapsed
}

class CsfNodeLabel implements vscode.TreeItemLabel {
  label: string;
  highlights?: [number, number][] | undefined;
  constructor (label: string, highlights?: [number, number][]) {
    this.label = label
    this.highlights = highlights
  }
}

export class CsfNode extends vscode.TreeItem {
  public children: CsfNode[] | undefined;
  private _parent: CsfNode | undefined
  public get parent (): CsfNode | undefined {
    return this._parent
  }

  private set parent (value: CsfNode | undefined) {
    this._parent = value
  }

  private _data?: CsfUnit | undefined;
  public get data (): CsfUnit | undefined {
    return this._data
  }

  public set data (value: CsfUnit | undefined) {
    this._data = value
  }

  /**
   * 创建一个Type节点
   * @param label 标签
   * @param children 子节点
   */
  private constructor (label: string | CsfNodeLabel, children: CsfNode[])
  /**
   * 创建一个Data节点
   * @param label 标签
   * @param data 数据
   */
  private constructor (label: string | CsfNodeLabel, data: CsfUnit)
  private constructor (label: string | CsfNodeLabel, arg2: CsfNode[] | CsfUnit) {
    super(label, getCollapsibleState(arg2) ? vscode.TreeItemCollapsibleState.Collapsed : vscode.TreeItemCollapsibleState.None)
    if (arg2 instanceof Array) {
      this.children = arg2
      arg2.forEach(child => { child.parent = this })
    } else if ('label' in arg2 && 'values' in arg2) {
      this.data = arg2
      // Command
      this.command = {
        title: 'Open',
        command: _viewCommand(CsfValueViewProvider.viewType, 'put'),
        arguments: [this],
        tooltip: 'Open in Edit Panel'
      }
    }
  }

  /**
   * 从数据创建一个数据节点
   * @param csf CsfUnit
   */
  static create(csf: CsfUnit): CsfNode
  /**
   * 创建一个Type节点
   * @param type 类型
   * @param children 成员
   */
  static create (type: string, children: CsfNode[]): CsfNode
  static create (arg1: string | CsfUnit, arg2?: CsfNode[]): CsfNode | undefined {
    if (typeof arg1 === 'string' && arg2) {
      return new CsfNode(arg1.toUpperCase(), arg2)
    }

    const csf = arg1 as CsfUnit
    return new CsfNode(csf.label, csf)
  }
}
