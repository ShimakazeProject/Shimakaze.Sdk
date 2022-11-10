import * as vscode from 'vscode'
import { CsfNode } from './CsfNode'

export class CsfLabelViewProvider implements vscode.TreeDataProvider<CsfNode> {
  public static readonly viewType = 'shimakaze-sdk.view.csf.label'
  public data?: CsfNode[] = []

  getTreeItem (element: CsfNode): vscode.TreeItem {
    return element
  }

  getChildren (element?: CsfNode): vscode.ProviderResult<CsfNode[]> {
    if (element === undefined) {
      return this.data
    }
    return element.children
  }

  getParent (element: CsfNode): vscode.ProviderResult<CsfNode> {
    return element.parent
  }

  private _onDidChangeTreeData = new vscode.EventEmitter<CsfNode | undefined | null | void>();
  readonly onDidChangeTreeData = this._onDidChangeTreeData.event;
  refresh = () => this._onDidChangeTreeData.fire()
}
