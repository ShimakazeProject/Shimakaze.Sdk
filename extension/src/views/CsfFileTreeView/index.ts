import * as vscode from 'vscode'

class DataItem extends vscode.TreeItem {
    public children: DataItem[] | undefined;

    constructor (label: string, children?: DataItem[] | undefined) {
      super(label, children === undefined ? vscode.TreeItemCollapsibleState.None : vscode.TreeItemCollapsibleState.Collapsed)
      this.children = children
    }
}

export class CsfFileProvider implements vscode.TreeDataProvider<DataItem> {
  static viewType= 'shimakaze-sdk.csf.file.treeview'

  private data: DataItem[] = []

  constructor () {
    this.data = [
      new DataItem('THEME', [new DataItem('THEME:Intro'), new DataItem('THEME:MadRap')]),
      new DataItem('DESC', [new DataItem('DESC:E31'), new DataItem('DESC:E32')]),
      new DataItem('MSG', [new DataItem('MSG:PingInfo'), new DataItem('MSG:Critical')])
    ]
  }

  getTreeItem (element: DataItem): vscode.TreeItem {
    return element
  }

  getChildren (element?: DataItem): vscode.ProviderResult<DataItem[]> {
    if (element === undefined) {
      return this.data
    }
    return element.children
  }
}
